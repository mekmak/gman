using System;
using Mekmak.Gman.Silk.Interfaces;

namespace Mekmak.Gman.UI.Iron
{
    public class ConsoleTraceLogger : BaseTraceLogger, ITraceLogger
    {
        private readonly string _moduleName;

        public ConsoleTraceLogger(string moduleName)
        {
            _moduleName = moduleName;
        }

        protected override string ModuleName => _moduleName;

        public ITraceLogger GetSubLogger(string subModuleName)
        {
            return new ConsoleTraceLogger($"{_moduleName}.{subModuleName}");
        }

        public void Info(string traceId, string logTag, string message = null)
        {
            Console.WriteLine(BuildConsoleLogMessage(Severity.Info, traceId, logTag, message));
        }

        public void Error(string traceId, string logTag, string message = null)
        {
            Console.WriteLine(BuildConsoleLogMessage(Severity.Error, traceId, logTag, message));
        }

        protected string BuildConsoleLogMessage(Severity severity, string traceId, string logTag, string message = null)
        {
            string innerMessage = BuildLogMessage(traceId, logTag, message);
            return $"[{DateTime.Now:HH:mm:ss.fff}] [{severity}] {innerMessage}";
        }

        protected enum Severity
        {
            Info,
            Error
        }
    }
}
