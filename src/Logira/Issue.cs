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

        internal Issue(string key)
        {
            Key = key;
        }
    }
}