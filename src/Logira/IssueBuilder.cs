using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Brevity;
using Logira.ServiceV2;

namespace Logira
{
    /// <summary>
    /// Responsible for setting issue properties and creating the issue. 
    /// </summary>
    public class IssueBuilder
    {
        #region fields
        private int _issueType;
        private string _summary;
        private Exception _exception;
        private string _projectKey;
        private readonly StringBuilder _description = new StringBuilder();
        private readonly List<Tuple<string, string>> _attachments = new List<Tuple<string, string>>();
        private readonly List<Tuple<int, string[]>> _customFields = new List<Tuple<int, string[]>>();
        private readonly List<string> _affectsVersionNames = new List<string>();
        private readonly StringBuilder _environment = new StringBuilder();

        #endregion

        #region issue "properties"
        /// <summary>
        /// Set the issue project key.
        /// </summary>
        /// <param name="projectKey"></param>
        /// <returns></returns>
        public IssueBuilder Project(string projectKey)
        {
            _projectKey = projectKey;
            return this;
        }

        /// <summary>
        /// Set the issue type. <see cref="IssueType"/> defines some useful defaults. Defaults to <see cref="IssueType.Bug"/> (id = 1).
        /// </summary>
        /// <param name="issueType"></param>
        /// <returns></returns>
        public IssueBuilder Type(int issueType)
        {
            _issueType = issueType;
            return this;
        }

        /// <summary>
        /// Set the issue summary.
        /// </summary>
        /// <param name="summary"></param>
        /// <returns></returns>
        public IssueBuilder Summary(string summary)
        {
            _summary = summary;
            return this;
        }

        /// <summary>
        /// Set the issue description.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public IssueBuilder Description(string description)
        {
            _description.AppendLine(description);
            return this;
        }

        /// <summary>
        /// Set the given exception as the issue description.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public IssueBuilder Description(Exception exception)
        {
            _exception = exception;
            return this;
        }

        /// <summary>
        /// Adds the given macro to the description. The macro is rendered immediately. 
        /// </summary>
        /// <typeparam name="TMacro"></typeparam>
        /// <param name="macro"></param>
        /// <returns></returns>
        public IssueBuilder Description<TMacro>(TMacro macro) where TMacro : class, IMacro
        {
            _description.AppendLine(macro.Render());

            return this;
        }

        /// <summary>
        /// Upload an attachment for the issue.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <param name="base64EncodedBinary">Base64 encoded binary. </param>
        /// <returns></returns>
        public IssueBuilder Attachment(string filename, string base64EncodedBinary)
        {
            _attachments.Add(new Tuple<string, string>(filename, base64EncodedBinary));
            return this;
        }

        /// <summary>
        /// Set custom field values for the issue.
        /// </summary>
        /// <param name="customFieldId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IssueBuilder CustomField(int customFieldId, params string[] value)
        {
            _customFields.Add(new Tuple<int, string[]>(customFieldId, value));
            return this;
        }

        /// <summary>
        /// Set the name of the version that affects the issue. 
        /// If the version doesn't exist, it is created. This requires that the JIRA user has administrator rights in the project. 
        /// </summary>
        /// <param name="versionName"></param>
        /// <param name="additionalVersionNames"> </param>
        /// <returns></returns>
        public IssueBuilder AffectsVersion(string versionName, params string[] additionalVersionNames)
        {
            _affectsVersionNames.Add(versionName);
            _affectsVersionNames.AddRange(additionalVersionNames);

            return this;
        }

        /// <summary>
        /// Declare the name of the version by retrieving information from the executing assembly. 
        /// If the version doesn't exist, it is created. This requires that the JIRA user has administrator rights in the project. 
        /// </summary>
        /// <returns></returns>
        public VersionBuilder AffectsVersion()
        {
            return new VersionBuilder(Assembly.GetCallingAssembly(), this);
        }

        /// <summary>
        /// Set the environment for the issue. 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public IssueBuilder Environment(string environment)
        {
            _environment.AppendLine(environment);

            return this;
        }
        #endregion

        /// <summary>
        /// Create the issue.
        /// </summary>
        /// <returns>Returns the created issue. The issue will contain the generated issue key.</returns>
        public Issue Create()
        {
            if (string.IsNullOrEmpty(_summary))
                throw new InvalidOperationException("Summary is required");
            if (string.IsNullOrEmpty(_projectKey))
                throw new InvalidOperationException("ProjectKey is required");

            var remoteAffectsVersions = CreateAffectsVersions();
            var remoteIssue = CreateRemoteIssue(remoteAffectsVersions);
            var remoteAttachments = CreateRemoteAttachments();

            var token = Jira.GetToken();

            remoteIssue = Jira.CreateIssue(token, remoteIssue);

            if (remoteAttachments != null)
                Jira.AddAttachments(token, remoteIssue, remoteAttachments);

            return new Issue(remoteIssue.key);
        }

        /// <summary>
        /// Gets the remote versions necessary for the affects versions. The versions are created if they don't exist. 
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<RemoteVersion> CreateAffectsVersions()
        {
            IEnumerable<RemoteVersion> remoteVersions = null;

            if (_affectsVersionNames.Count != 0)
                remoteVersions = _affectsVersionNames
                    .Select(versionName => Jira.GetVersion(_projectKey, versionName) ?? Jira.CreateVersion(_projectKey, versionName));

            return remoteVersions;
        }

        /// <summary>
        /// Creates "remote" attachments based on the attachments set in the IssueBuilder. 
        /// </summary>
        /// <returns></returns>
        internal Tuple<string[], string[]> CreateRemoteAttachments()
        {
            if (_attachments.Count == 0)
                return null;

            var names = new string[_attachments.Count];
            var files = new string[_attachments.Count];

            for (var i = 0; i < _attachments.Count; i++)
            {
                names[i] = _attachments[i].Item1;
                files[i] = _attachments[i].Item2;
            }

            return new Tuple<string[], string[]>(names, files);
        }

        /// <summary>
        /// Creates a remote issue object based on the properties set in the IssueBuilder. 
        /// </summary>
        /// <param name="remoteAffectsVersions"> </param>
        /// <returns></returns>
        internal RemoteIssue CreateRemoteIssue(IEnumerable<RemoteVersion> remoteAffectsVersions = null)
        {
            var issue = new RemoteIssue
                            {
                                summary = _summary.Truncate(Jira.MaxSummaryLength),
                                description = _description.ToString(),
                                project = _projectKey,
                                type = _issueType == 0 ? IssueType.Bug.ToString(CultureInfo.InvariantCulture) : _issueType.ToString(CultureInfo.InvariantCulture),
                                environment = _environment.ToString(),
                            };

            if (_summary != null && _summary.Length > Jira.MaxSummaryLength)
            {
                if (string.IsNullOrEmpty(issue.description)) //description not already set
                    issue.description = _summary;
                else //description already set
                    issue.description = issue.description.Insert(0, _summary + "\n\n");
            }

            #region set description from exception
            if (_exception != null)
            {
                var message = new StringBuilder();
                var stacktrace = new StringBuilder();

                issue.description += "\n{code:title=Exception}\n";

                message.Append(
                    "$type$: $message$"
                        .Set("type", _exception.GetType().FullName)
                        .Set("message", _exception.Message));

                if (_exception.StackTrace != null)
                {
                    stacktrace.AppendLine();
                    stacktrace.Append(_exception.StackTrace);
                }

                var inner = _exception.InnerException;

                while (inner != null)
                {
                    message.Append(
                        " ---> $type$: $message$"
                            .Set("type", inner.GetType().FullName)
                            .Set("message", inner.Message));

                    if (inner.StackTrace != null)
                    {
                        stacktrace.Insert(0, "\n   --- End of inner exception stack trace ---");
                        stacktrace.Insert(0, inner.StackTrace);
                        stacktrace.Insert(0, "\n");
                    }

                    inner = inner.InnerException;
                }

                message.AppendLine();

                issue.description += message;
                issue.description += stacktrace;
                issue.description += "\n{code}";
            }
            #endregion

            if (remoteAffectsVersions != null)
                issue.affectsVersions = remoteAffectsVersions.ToArray();

            if (_customFields.Count != 0)
            {
                issue.customFieldValues = new RemoteCustomFieldValue[_customFields.Count];

                for (var i = 0; i < _customFields.Count; i++)
                {
                    issue.customFieldValues[i] = new RemoteCustomFieldValue
                                                     {
                                                         customfieldId = _customFields[i].Item1.ToString(CultureInfo.InvariantCulture),
                                                         values = _customFields[i].Item2
                                                     };
                }
            }

            return issue;
        }

        #region versionbuilder
        /// <summary>
        /// Enables specifying how version name should be extracted from the assembly.
        /// </summary>
        public class VersionBuilder
        {
            private readonly Assembly _assembly;
            private readonly IssueBuilder _issueBuilder;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="assembly">The calling assembly.</param>
            /// <param name="issueBuilder"></param>
            internal VersionBuilder(Assembly assembly, IssueBuilder issueBuilder)
            {
                _assembly = assembly;
                _issueBuilder = issueBuilder;
            }

            /// <summary>
            /// Use the entire informational version.
            /// </summary>
            /// <returns></returns>
            public IssueBuilder AssemblyInformationalVersion()
            {
                return _issueBuilder.AffectsVersion(
                    GetVersion<AssemblyInformationalVersionAttribute>(attribute => attribute.InformationalVersion));
            }

            /// <summary>
            /// Specify what parts of the informational version to use. 
            /// </summary>
            /// <returns></returns>
            public IssueBuilder AssemblyInformationalVersion(Func<Version, string> resolveVersionName)
            {
                return _issueBuilder.AffectsVersion(
                    resolveVersionName(
                        new Version(GetVersion<AssemblyInformationalVersionAttribute>(attribute => attribute.InformationalVersion))));
            }

            /// <summary>
            /// Use the entire assembly version.
            /// </summary>
            /// <returns></returns>
            public IssueBuilder AssemblyVersion()
            {
                return _issueBuilder.AffectsVersion(
                    GetVersion<AssemblyVersionAttribute>(attribute => attribute.Version));
            }

            /// <summary>
            /// Specify what parts of the assembly version to use. 
            /// </summary>
            /// <returns></returns>
            public IssueBuilder AssemblyVersion(Func<Version, string> resolveVersionName)
            {
                return _issueBuilder.AffectsVersion(
                    resolveVersionName(
                        new Version(GetVersion<AssemblyVersionAttribute>(attribute => attribute.Version))));
            }

            /// <summary>
            /// Use the entire file version.
            /// </summary>
            /// <returns></returns>
            public IssueBuilder AssemblyFileVersion()
            {
                return _issueBuilder.AffectsVersion(
                    GetVersion<AssemblyFileVersionAttribute>(attribute => attribute.Version));
            }

            /// <summary>
            /// Specify what parts of the file version to use. 
            /// </summary>
            /// <returns></returns>
            public IssueBuilder AssemblyFileVersion(Func<Version, string> resolveVersionName)
            {
                return _issueBuilder.AffectsVersion(
                   resolveVersionName(
                       new Version(GetVersion<AssemblyFileVersionAttribute>(attribute => attribute.Version))));
            }

            /// <summary>
            /// Specify how version should be extracted from the executing assembly. 
            /// </summary>
            /// <param name="resolveVersionName"></param>
            /// <returns></returns>
            public IssueBuilder Assembly(Func<Assembly, string> resolveVersionName)
            {
                return _issueBuilder.AffectsVersion(resolveVersionName(_assembly));
            }

            private string GetVersion<TCustomAttribute>(Func<TCustomAttribute, string> getVersion) where TCustomAttribute : class
            {
                var attribute = _assembly.GetCustomAttribute<TCustomAttribute>();

                if (attribute == null)
                    throw new ArgumentException("Attribute {0} is not declared on assembly {1}".FormatWith(typeof(TCustomAttribute).FullName, _assembly.FullName));

                return getVersion(attribute);
            }
        }
        #endregion

        /// <summary>
        /// Set environment using info on the server or the client.
        /// </summary>
        /// <returns></returns>
        public EnvironmentBuilder Environment()
        {
            return new EnvironmentBuilder(this);
        }

        /// <summary>
        /// Set environment using info on the server or the client.
        /// </summary>
        public class EnvironmentBuilder
        {
            private readonly IssueBuilder _issueBuilder;

            internal EnvironmentBuilder(IssueBuilder issueBuilder)
            {
                _issueBuilder = issueBuilder;
            }

            /// <summary>
            /// Adds info from <see cref="System.Environment"/> to the issue's environment.
            /// </summary>
            public IssueBuilder FromServer()
            {
                _issueBuilder._environment.AppendLine("************ Server ************");
                _issueBuilder._environment.AppendLine("\tCommandLine: " + System.Environment.CommandLine);
                _issueBuilder._environment.AppendLine("\tCurrentDirectory: " + System.Environment.CurrentDirectory);
                _issueBuilder._environment.AppendLine("\tIs64BitOperatingSystem: " + System.Environment.Is64BitOperatingSystem);
                _issueBuilder._environment.AppendLine("\tIs64BitProcess: " + System.Environment.Is64BitProcess);
                _issueBuilder._environment.AppendLine("\tMachineName: " + System.Environment.MachineName);
                _issueBuilder._environment.AppendLine("\tOSVersion: " + System.Environment.OSVersion);
                _issueBuilder._environment.AppendLine("\tProcessorCount: " + System.Environment.ProcessorCount);
                _issueBuilder._environment.AppendLine("\tSystemDirectory: " + System.Environment.SystemDirectory);
                _issueBuilder._environment.AppendLine("\tSystemPageSize: " + System.Environment.SystemPageSize);
                _issueBuilder._environment.AppendLine("\tTickCount: " + System.Environment.TickCount);
                _issueBuilder._environment.AppendLine("\tUserDomainName: " + System.Environment.UserDomainName);
                _issueBuilder._environment.AppendLine("\tUserInteractive: " + System.Environment.UserInteractive);
                _issueBuilder._environment.AppendLine("\tUserName: " + System.Environment.UserName);
                _issueBuilder._environment.AppendLine("\tVersion: " + System.Environment.Version);
                _issueBuilder._environment.AppendLine("\tWorkingSet: " + System.Environment.WorkingSet);

                return _issueBuilder;
            }

            /// <summary>
            /// Adds info from <see cref="HttpRequest"/> to the issue's environment.
            /// </summary>
            public IssueBuilder FromClient(HttpRequest request = null)
            {
                if (request == null)
                {
                    if (HttpContext.Current != null)
                        request = HttpContext.Current.Request;
                    else
                        throw new ArgumentException("Request is neither specified as argument nor found in the current HttpContext.");
                }

                _issueBuilder._environment.AppendLine("************ Client ************");
                _issueBuilder._environment.AppendLine(request.ToJson());

                return _issueBuilder;
            }
        }     
    }
}