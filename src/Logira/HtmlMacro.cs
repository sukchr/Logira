using Brevity.StringTemplate;

namespace Logira
{
    /// <summary>
    /// Wraps the <see cref="Contents"/> inside {html}. 
    /// </summary>
    public class HtmlMacro : IMacro
    {
        /// <summary>
        /// The contents of the html macro.
        /// </summary>
        public string Contents { get; set; }

        public string Render()
        {
            return @"{html}$contents${html}"
                .Set("contents", Contents);
        }
    }
}