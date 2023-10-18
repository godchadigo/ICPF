using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFCJsonRPC.Model
{
    public class OperactionResult
    {
        public string Uuid { get; set; }
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
    public class OperationResult<T>
    {
        public string Uuid { get; set; }
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
