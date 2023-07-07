using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFC
{
    public interface IPFC
    {
        void Connect();
        void Connect(string ip_port);
        OperationModel Send(string cmd);
        OperationModel GetData(ReadDataModel model);
        OperationModel SetData(WriteDataModel model);
    }
}
