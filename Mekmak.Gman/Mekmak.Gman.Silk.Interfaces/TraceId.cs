using System;
using System.Collections.Generic;
using System.Text;

namespace Mekmak.Gman.Silk.Interfaces
{
    public class TraceId
    {
        public static string New()
        {
            return Guid.NewGuid().ToString().Split('-')[0];
        }

        public static string New(string baseTraceId)
        {
            return $"{baseTraceId}.{New()}";
        }
    }
}
