using System;

namespace SmtpServer
{
    public interface ILogger
    {
        void WriteTrace(string message, params object[] args);
        void WriteDebug(string message, params object[] args);
        void WriteInfo(string message, params object[] args);
        void WriteWarning(string message, params object[] args);
        void WriteError(string message, params object[] args);

        string WriteException(Exception exception);
        string WriteException(Exception exception, string message, params object[] args);
        string WriteExceptionWarning(Exception exception);
    }
}