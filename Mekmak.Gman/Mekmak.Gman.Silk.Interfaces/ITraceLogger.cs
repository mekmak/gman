namespace Mekmak.Gman.Silk.Interfaces
{
    public interface ITraceLogger
    {
        ITraceLogger GetSubLogger(string subModuleName);

        void Info(string traceId, string logTag, string message = null);
        void Error(string traceId, string logTag, string message = null);
    }
}
