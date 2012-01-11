using Brevity;

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
            return @"{html}\n$contents$\n{html}"
                .Set("contents", Contents);
        }
    }
}