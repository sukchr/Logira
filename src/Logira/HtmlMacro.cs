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
<<<<<<< HEAD
            return @"{html}$contents${html}"
=======
            return @"{html}\n$contents$\n{html}"
>>>>>>> 697f96607b2a4de5c254b3f5a51067316784419c
                .Set("contents", Contents);
        }
    }
}