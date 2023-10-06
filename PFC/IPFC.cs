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
        Task<OperationModel> GetData(ReadDataModel model);
        Task<OperationModel> SetData(WriteDataModel model);
        Task<OperationModel> GetTag(string deviceName, string tagName);
        Task<OperationTagGroupModel> GetTagGroup(string deviceName, string groupName);
        Task<OperationModel> GetMachins();

    }
}
