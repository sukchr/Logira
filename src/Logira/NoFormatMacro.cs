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
            return @"{noformat}$contents${noformat}"
                .Set("contents", Contents);
        }
    }
}