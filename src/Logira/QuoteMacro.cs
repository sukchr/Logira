using System;
using Brevity;

namespace Logira
{
    /// <summary>
    /// Wraps the <see cref="Quote"/> inside {quote}. 
    /// </summary>
    public class QuoteMacro : IMacro
    {
        /// <summary>
        /// The contents of the quote macro
        /// </summary>
        public string Quote { get; set; }

        public string Render()
        {
            return @"{quote}\n$quote$\n{quote}"
                .Set("quote", Quote);
        }
    }
}