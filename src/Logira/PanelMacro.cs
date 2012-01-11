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
            return @"{panel:title=$title$}$contents${panel}"
                .Set("title", Title)
                .Set("contents", Contents);
        }
    }
}