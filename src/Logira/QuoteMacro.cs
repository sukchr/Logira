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
<<<<<<< HEAD
            return @"{quote}$quote${quote}"
=======
            return @"{quote}\n$quote$\n{quote}"
>>>>>>> 697f96607b2a4de5c254b3f5a51067316784419c
                .Set("quote", Quote);
        }
    }
}