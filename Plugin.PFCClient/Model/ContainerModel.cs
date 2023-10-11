using ICPFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.PFCClient.Model
{

    public class ContainerModel
    {
        public string Uuid { get; set; }
        public string DeviceName { get; set; }
        public string TagGroup { get; set; }
        public string TagName { get; set; }
        public QJDataArray Data { get; set; }

    }
    public class ContainerModelPacket
    {
        public string Uuid { get; set; }
        public List<ContainerModel> Container { get; set; }
    }
}
