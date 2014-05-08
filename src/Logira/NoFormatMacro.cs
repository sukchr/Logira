using Brevity.StringTemplate;

namespace Logira
{
    /// <summary>
    /// Wraps the <see cref="Contents"/> inside {noformat}. 
    /// </summary>
    public class NoFormatMacro : IMacro
    {
        /// <summary>
        /// The macro's title: {noformat:title=The title}
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The contents of the noformat macro.
        /// </summary>
        public string Contents { get; set; }

        public string Render()
        {
            return @"{noformat:title=$title$}$contents${noformat}"
                .Set("contents", Contents)
                .Set("title", Title);
        }
    }
}