using System;

namespace SmtpServer
{
    internal sealed class NullLogger : ILogger
    {
        public void WriteTrace(string message, params object[] args) { }
        public void WriteDebug(string message, params object[] args) { }
        public void WriteInfo(string message, params object[] args) { }
        public void WriteWarning(string message, params object[] args) { }
        public void WriteError(string message, params object[] args) { }

        public string WriteException(Exception exception)
        {
            return string.Empty;
        }

        public string WriteException(Exception exception, string message, params object[] args)
        {
            return string.Empty;
        }

        public string WriteExceptionWarning(Exception exception)
        {
            return string.Empty;
        }
    }
}