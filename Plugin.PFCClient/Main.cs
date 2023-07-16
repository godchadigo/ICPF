using System.Net;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Rpc.WebApi;
using TouchSocket.Sockets;
using ICPFCore;

namespace PluginPFCClient
{
    public class Main : ICPFCore.PluginBase
    {
        public override string PluginName { get; set; } = "PFC_Plugin";
        
        private TcpService service = new TcpService();
        private CancellationTokenSource cts = new CancellationTokenSource();
        Thread t1;
        public override void onLoading()
        {
            base.onLoading();
            
            t1 = new Thread(() =>
            {                
                service.Connecting = (client, e) => { };//有客户端正在連接
                service.Connected = (client, e) => { };//有客户端成功連接
                service.Disconnected = (client, e) => { };//有客户端段開連接
                service.Received = (client, byteBlock, requestInfo) =>
                {
                    //从客户端收到信息
                    string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);

                    if (false)
                    {
                        client.Logger.Info("###################Start#################\r\n");
                        client.Logger.Info(mes + "\r\n");
                        client.Logger.Info("####################End##################\r\n");
                    }
                    
                    //Console.WriteLine(mes);
                    var packRes = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseDataModel>(mes);
                    
                    if (packRes.iRWDataOperation == IRWDataOperation.Read)
                    {
                        try
                        {
                            var readModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ReadDataModel>(mes);
                            //client.Logger.Info($"地址:{readModel.Address}");
                            var value = GetData(readModel).Result;
                            var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                            client.Send(jsonStr);
                            //client.Logger.Info(jsonStr);
                        }
                        catch (Exception ex) { }
                    }
                    if (packRes.iRWDataOperation == IRWDataOperation.Write)
                    {
                        try
                        {
                            var writeModel = Newtonsoft.Json.JsonConvert.DeserializeObject<WriteDataModel>(mes);
                            var value = SetData(writeModel).Result;
                            var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                            client.Send(jsonStr);
                            //client.Logger.Info(jsonStr);
                            //client.Logger.Info($"地址:{writeModel.Address}");
                        }
                        catch (Exception ex) 
                        {
                            client.Logger.Info("Error : " + ex.Message);
                        }
                    }       
                };
                //Console.WriteLine("------------");
                service.Setup(new TouchSocketConfig()//载入配置     
                    .SetListenIPHosts(new IPHost[] { new IPHost(5000) })
                    .ConfigureContainer(a =>
                    {
                        a.AddConsoleLogger();
                    })
                    .ConfigurePlugins(a =>
                    {
                        
                    })
                    .SetDataHandlingAdapter(() => { return new TerminatorPackageAdapter("\r\n"); }))//配置终止字符適配器，以\r\n结尾。                                    
                    .Start();//启动                
                
            });
            t1.IsBackground = true;
            t1.Start();
        }
        public override void onCloseing()
        {            
            service.Stop();
            service.Dispose();
            service = null;            
            t1.Interrupt();
            t1 = null;
            base.onCloseing();
        }        
    }
}
