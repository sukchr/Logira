using Brevity;

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
<<<<<<< HEAD
            return @"{code:title=$title$}$code${code}"
=======
            return @"{code:title=$title$}\n$code$\n{code}"
>>>>>>> 697f96607b2a4de5c254b3f5a51067316784419c
                .Set("title", Title)
                .Set("code", Code);
        }
    }
}