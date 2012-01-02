using System;
using System.Text;
using Brevity;
using log4net.Appender;
using log4net.Core;
using log4net.Util;
using Logira.ServiceV2;

namespace Logira
{
    /// <summary>
    /// Creates a JIRA issue.
    /// </summary>
    public sealed class JiraAppender : AppenderSkeleton
    {
        /// <summary>
        /// The URL to the JIRA SOAP service. Required.
        /// </summary>
        /// <example>
        /// https://yoursite.com/jira/rpc/soap/jirasoapservice-v2
        /// </example>
        public string Url { get; set; }
        /// <summary>
        /// The username to log into JIRA with. Required.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The password to log into JIRA with. Required.
        /// </summary>
        public String Password { get; set; }
        /// <summary>
        /// The id of the issue type. Required. 
        /// </summary>
        public string IssueTypeId { get; set; }
        /// <summary>
        /// The project key.
        /// </summary>
        public string ProjectKey { get; set; }
        /// <summary>
        /// The id of the component. Optional.
        /// </summary>
        public string ComponentId { get; set; }
        /// <summary>
        /// The username of the assignee. Optional.
        /// </summary>
        public string AssigneeUsername { get; set; }
        /// <summary>
        /// The max number of chars to display in the summary.
        /// </summary>
        private const int SummaryMaxCharCount = 50;

        protected override void Append(LoggingEvent loggingEvent)
        {
            var summary = Layout != null ? RenderLoggingEvent(loggingEvent) : loggingEvent.MessageObject.ToString();

            var issue = new RemoteIssue
            {
                project = ProjectKey,
                type = IssueTypeId,
                summary = summary.Truncate(SummaryMaxCharCount)
            };
            if (!string.IsNullOrEmpty(AssigneeUsername)) issue.assignee = AssigneeUsername;
            if (!string.IsNullOrEmpty(ComponentId)) issue.components = new[] { new RemoteComponent { id = ComponentId } };
            var description = new StringBuilder();
            if (summary.Length > SummaryMaxCharCount) description.AppendFormat("Message: {0}\n", summary);
            description.AppendFormat("Level: {0}\n", loggingEvent.Level);
            description.AppendFormat("Logger: {0}\n", loggingEvent.LoggerName);
            if (loggingEvent.ExceptionObject != null)
            {
                description.Append("\n{code:title=Exception}");
                description.Append(loggingEvent.GetExceptionString());
                description.Append("{code}");
            }
            issue.description = description.ToString();

            var service = new JiraSoapServiceService { Url = Url };
            var token = service.login(Username, Password);
            LogLog.Debug(typeof(JiraAppender), string.Format("Sending request to JIRA: {0}", issue.ToJson()));
            var returnedIssue = service.createIssue(token, issue);
            LogLog.Debug(typeof(JiraAppender), string.Format("Got response from JIRA: {0}", returnedIssue.ToJson()));
            LogLog.Debug(typeof(JiraAppender), string.Format("Created issue: {0}", returnedIssue.key));
        }
    }
}
