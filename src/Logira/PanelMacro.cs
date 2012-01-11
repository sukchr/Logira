using Brevity;

namespace Logira
{
    /// <summary>
    /// Wraps the <see cref="Contents"/> inside {panel}. 
    /// </summary>
    public class PanelMacro : IMacro
    {
        /// <summary>
        /// The panel macro's title: {panel:title=The title}
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The contents of the panel macro.
        /// </summary>
        public string Contents { get; set; }

        public string Render()
        {
<<<<<<< HEAD
            return @"{panel:title=$title$}$contents${panel}"
=======
            return @"{panel:title=$title$}\n$contents$\n{panel}"
>>>>>>> 697f96607b2a4de5c254b3f5a51067316784419c
                .Set("title", Title)
                .Set("contents", Contents);
        }
    }
}