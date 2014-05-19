using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Graphite
{
    internal static class Logging
    {
        private static readonly TraceSource source = new TraceSource("Graphite");
        private static readonly string startLine = new string('=', 30);
        private static readonly string secondLine = new string('-', 30);

        /// <summary>
        /// Graphite trace source.
        /// </summary>
        public static TraceSource Source
        {
            get { return source; }
        }

        public static string Format(this Exception exception)
        {
            if (exception == null)
                return null;

            var buffer = new StringBuilder();

            buffer.AppendLine(startLine);
            buffer.AppendLine(DateTime.Now.ToString(CultureInfo.InvariantCulture));
            buffer.AppendLine(secondLine);

            Exception current = exception;

            int intend = 0;
            do
            {
                Format(current, buffer, intend);

                current = exception.InnerException;
                intend += 4;

            } while (current != null);

            return buffer.ToString();
        }

        private static void Format(Exception exception, StringBuilder buffer, int intend = 0)
        {
            var indent = new string(' ', intend);
            buffer.Append(indent).AppendLine(exception.Message);
            buffer.AppendLine();
            buffer.Append(indent).AppendLine(exception.StackTrace);
            buffer.Append(indent).AppendLine(60 - intend > 0 ? new string('-', 60 - intend) : string.Empty);
            buffer.AppendLine();
        }
    }
}
