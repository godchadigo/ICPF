using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFCJsonRPC.Model
{
    public enum DataType
    {
        Bool = 1,
        UInt16 = 2,
        Int16 = 3,
        UInt32 = 4,
        Int32 = 5,
        UInt64 = 6,
        Int64 = 7,
        Float = 8,
        Double = 9,
        String = 10,
    }
    public class DataModel
    {
    }
    /// <summary>
    /// QJData 特殊數據標記類
    /// QJData v1.基本資料類型標記
    /// </summary>
    public class QJData
    {
        public string Uuid { get; set; }
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public string Address { get; set; }
        public object Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
    }
    public class QJTagData : QJData
    {
        public string GroupName { get; set; }
        public string TagName { get; set; }
    }
}
