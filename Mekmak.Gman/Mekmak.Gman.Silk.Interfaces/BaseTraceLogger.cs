namespace Mekmak.Gman.Silk.Interfaces
{
    public abstract class BaseTraceLogger
    {
        protected abstract string ModuleName { get; }

        protected string BuildLogMessage(string traceId, string logTag, string message = null)
        {
            return string.IsNullOrWhiteSpace(message)
                ? $"logTag={ModuleName}.{logTag}, traceId={traceId}"
                : $"logTag={ModuleName}.{logTag}, traceId={traceId}, {message}";
        }
    }
}
