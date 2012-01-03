using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Brevity;
using Logira.ServiceV2;

namespace Logira
{
    /// <summary>
    /// Responsible for setting issue properties and creating the issue. 
    /// </summary>
    public class IssueBuilder
    {
        private int _issueType;
        private string _summary;
        private Exception _exception;
        private string _projectKey;
        private string _description;
        private readonly List<Tuple<string, string>> _attachments = new List<Tuple<string, string>>();
        private readonly List<Tuple<int, string[]>> _customFields = new List<Tuple<int, string[]>>();

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
            _description = description;
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
        /// Create the issue.
        /// </summary>
        /// <returns>Returns the created issue. The issue will contain the generated issue key.</returns>
        public Issue Create()
        {
            if (string.IsNullOrEmpty(_summary))
                throw new InvalidOperationException("Summary is required");
            if (string.IsNullOrEmpty(_projectKey))
                throw new InvalidOperationException("ProjectKey is required");

            var remoteIssue = CreateRemoteIssue();
            var remoteAttachments = CreateRemoteAttachments();
            
            var token = Jira.GetToken();
            
            remoteIssue = Jira.CreateIssue(token, remoteIssue);
            
            if(remoteAttachments != null)
                Jira.AddAttachments(token, remoteIssue, remoteAttachments);
            
            return new Issue(remoteIssue.key);
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
        /// <returns></returns>
        internal RemoteIssue CreateRemoteIssue()
        {
            var issue = new RemoteIssue
                            {
                                summary = _summary.Truncate(Jira.MaxSummaryLength),
                                description = _description,
                                project = _projectKey,
                                type = _issueType == 0 ? IssueType.Bug.ToString(CultureInfo.InvariantCulture) : _issueType.ToString(CultureInfo.InvariantCulture),
                            };

            if (_summary.Length > Jira.MaxSummaryLength)
            {
                if (string.IsNullOrEmpty(issue.description)) //description not already set
                    issue.description = _summary;
                else //description already set
                    issue.description = issue.description.Insert(0, _summary + "\n\n");
            }

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

                while(inner != null)
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
    }
}