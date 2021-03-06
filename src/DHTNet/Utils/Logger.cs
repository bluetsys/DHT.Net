using System;
using System.Text;

namespace DHTNet.Utils
{
    public static class Logger
    {
        private static readonly object _lockObj = new object();
        private static readonly StringBuilder _sb = new StringBuilder();

        public static void Log(string message)
        {
            Log(message, null);
        }

        public static void Log(string message, params object[] formatting)
        {
            lock (_lockObj)
            {
                _sb.Remove(0, _sb.Length);
                _sb.Append(DateTime.Now);
                _sb.Append(": ");
                _sb.Append(formatting != null ? string.Format(message, formatting) : message);

                string s = _sb.ToString();

                Console.WriteLine(s);
            }
        }
    }
}