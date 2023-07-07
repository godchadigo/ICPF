using PFC;
using System.Diagnostics;

namespace ICPFWebAPI.Services
{
    public class PFCService : IPFC
    {
        PFC.PFC pfc = new PFC.PFC();
        public PFCService()
        {
            //throw new NotImplementedException();
            //Debug.WriteLine("asfsfa");
            Connect();

        }
        public PFCService(string ip_port)
        {
            //throw new NotImplementedException();
            //Debug.WriteLine("asfsfa");
            Connect(ip_port);
            
        }
        public void Connect()
        {            
            pfc.Connect();
        }
        public void Connect(string ip_port)
        {
            pfc.Connect(ip_port);
        }
        public OperationModel GetData(ReadDataModel model)
        {
            return pfc.GetData(model);
        }

        public OperationModel Send(string cmd)
        {
            return pfc.Send(cmd);
        }

        public OperationModel SetData(WriteDataModel model)
        {
            return pfc.SetData(model);
        }
    }
}
