using System;
using Brevity;
using Logira.ServiceV2;
using log4net;

namespace Logira
{
    /// <summary>
    /// Responsible for configuring the JIRA connection.
    /// </summary>
    public class Jira
    {
        private static string _username, _password, _url;
        internal static JiraSoapServiceService Service;
        internal static bool IsConfigured;
        private static readonly ILog Log = LogManager.GetLogger(typeof(Jira).FullName);
        private const string SoapServiceUrl = "rpc/soap/jirasoapservice-v2";

        /// <summary>
        /// The maximum allowed length of the summary. The summary will be truncated if it is longer than this. The full summary will be 
        /// included in the issue description.
        /// </summary>
        public static int MaxSummaryLength = 50;
        internal static string BrowseIssueUrl;

        internal static string GetToken()
        {
            var token = Service.login(_username, _password);
            Log.DebugFormat("Service.login returned token: {0}", token.Mask());
            return token;
        }

        internal static RemoteIssue CreateIssue(string token, RemoteIssue remoteIssue)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("JIRA is not configured");

            if (Log.IsDebugEnabled)
                Log.DebugFormat("Creating issue:\n{0}", remoteIssue.ToJson());

            remoteIssue = Service.createIssue(token, remoteIssue);

            if (Log.IsDebugEnabled)
                Log.DebugFormat("Successfully created issue:\n{0}", remoteIssue.ToJson());

            return remoteIssue;
        }

        /// <summary>
        /// Adds attachments to a given issue.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="remoteIssue"></param>
        /// <param name="remoteAttachments">A tuple where the first string array is a list of names, the second array a base64 encoded list of files corresponding to the names.</param>
        internal static void AddAttachments(string token, RemoteIssue remoteIssue, Tuple<string[], string[]> remoteAttachments)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("JIRA is not configured");

            if (Log.IsDebugEnabled)
                Log.DebugFormat("Adding attachments to issue {0}: {1}", remoteIssue.key, remoteAttachments.Item1.Join());

            Service.addBase64EncodedAttachmentsToIssue(token, remoteIssue.key, remoteAttachments.Item1, remoteAttachments.Item2);
        }

        /// <summary>
        /// Invoke Configure to setup the JIRA connection. This method must be invoked before creating issues via <see cref="IssueBuilder"/>. 
        /// </summary>
        /// <param name="url">The URL of the JIRA SOAP service. E.g. https://your-jira-site.com</param>
        /// <param name="username">The username of the user to log into JIRA with.</param>
        /// <param name="password">The password of the user to log into JIRA with.</param>
        public static void Configure(string url, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;

            BrowseIssueUrl = url.EnsureTrailing("/") + "browse/";

            IsConfigured = true;
            Service = new JiraSoapServiceService { Url = url.EnsureTrailing("/") + SoapServiceUrl };
            Log.DebugFormat("JIRA was configured with url, username, password: '{0}', '{1}', '{2}'", _url, _username, _password.Mask());
        }
    }
}