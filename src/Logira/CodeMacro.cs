using Brevity.StringTemplate;

namespace Logira
{
    /// <summary>
    /// Wraps the <see cref="Code"/> inside {code}. 
    /// </summary>
    public class CodeMacro : IMacro
    {
        /// <summary>
        /// The code macro's title: {code:title=The title}
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The contents of the code macro.
        /// </summary>
        public string Code { get; set; }

        public string Render()
        {
            return @"{code:title=$title$}$code${code}"
                .Set("title", Title)
                .Set("code", Code);
        }
    }
}