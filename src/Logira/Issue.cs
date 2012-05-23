using Brevity;

namespace Logira
{
    /// <summary>
    /// Represents an Issue created by the <see cref="IssueBuilder"/>.
    /// </summary>
    public class Issue
    {
        /// <summary>
        /// Gets the key of the issue. E.g. "TST-123".
        /// </summary>
        public string Key { get; private set; }
        /// <summary>
        /// Gets the project key of the issue. E.g. "TST".
        /// </summary>
        public string ProjectKey { get; private set; }
        /// <summary>
        /// Gets the issue counter of the issue. E.g. "123". 
        /// </summary>
        public int IssueCounter { get; private set; }
        /// <summary>
        /// Gets the URL of the issue. Uses <see cref="Jira.BrowseIssueUrl"/> to build the full URL of the issue. 
        /// </summary>
        public string Url
        {
            get { return Jira.BrowseIssueUrl + Key; }
        }

        internal Issue(string key)
        {
            Key = key;
            var parts = key.Split('-');
            ProjectKey = parts[0];
            IssueCounter = parts[1].ToInt();
        }
    }
}