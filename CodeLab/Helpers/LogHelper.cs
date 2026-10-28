using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Hosting;

namespace CodeLab.Helpers
{
    /// <summary>
    /// Writes technical errors to App_Data/Logs/error-yyyyMMdd.log.
    /// For business activity ("who did what"), use AuditManager.RecordActivity instead.
    /// </summary>
    public static class LogHelper
    {
        private static readonly object _fileLock = new object();

        public static void LogError(string context, Exception exception)
        {
            StringBuilder entry = new StringBuilder();
            entry.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")).Append(" UTC | ").AppendLine(context);
            if (exception != null)
            {
                entry.AppendLine(exception.ToString());
            }
            entry.AppendLine();

            try
            {
                string logFolder = HostingEnvironment.MapPath("~/App_Data/Logs");
                string logFile = Path.Combine(logFolder, "error-" + DateTime.UtcNow.ToString("yyyyMMdd") + ".log");

                lock (_fileLock)
                {
                    Directory.CreateDirectory(logFolder);
                    File.AppendAllText(logFile, entry.ToString());
                }
            }
            catch (Exception)
            {
                // Logging must never crash the request it is reporting on
                Trace.TraceError(entry.ToString());
            }
        }
    }
}
