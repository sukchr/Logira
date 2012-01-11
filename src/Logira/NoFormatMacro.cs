using Brevity;

namespace Logira
{
    /// <summary>
    /// Wraps the <see cref="Contents"/> inside {noformat}. 
    /// </summary>
    public class NoFormatMacro : IMacro
    {
        /// <summary>
        /// The contents of the noformat macro.
        /// </summary>
        public string Contents { get; set; }

        public string Render()
        {
<<<<<<< HEAD
            return @"{noformat}$contents${noformat}"
=======
            return @"{noformat}\n$contents$\n{noformat}"
>>>>>>> 697f96607b2a4de5c254b3f5a51067316784419c
                .Set("contents", Contents);
        }
    }
}