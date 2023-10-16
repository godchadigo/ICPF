using HslCommunication.Core.Net;
using ICPFCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPF.Model
{
    public class DoubleNetworkBase
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public NetworkDeviceBase DeviceBase { get; set; }
        public Task Loop { get; set; }
        //public ConcurrentBag<QJData> Container { get; set; } = new ConcurrentBag<QJData>();
        public ConcurrentDictionary<string, QJTagData> Container { get; set; } = new ConcurrentDictionary<string, QJTagData>();
        public List<Tag> TagList { get; set; } = new List<Tag>();
        

        public void StartTask()
        {
            
        }
    }
}
