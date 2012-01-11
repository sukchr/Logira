namespace Logira
{
    /// <summary>
    /// See http://confluence.atlassian.com/display/JIRA/Editing+Rich-Text+Fields. 
    /// </summary>
    public interface IMacro
    {
        /// <summary>
        /// Returns the macro output.
        /// </summary>
        /// <returns></returns>
        string Render();
    }
}
