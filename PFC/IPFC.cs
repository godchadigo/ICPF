using System;
using System.Collections.Concurrent;
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
        Task<OperationResult<QJTagData>> GetTag(string deviceName, string tagName);
        Task<OperationResult<List<QJTagData>>> GetTagGroup(string deviceName, string groupName);
        Task<OperationResult<ConcurrentDictionary<string, QJTagData>>> GetContainer(string deviceName);
        Task<OperationResult<List<string>>> GetMachins();

    }
}
