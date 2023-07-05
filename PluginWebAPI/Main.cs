using ConsolePluginTest;
using System.Net;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Rpc.WebApi;
using TouchSocket.Sockets;

namespace PluginPFCClient
{
    public class Main : PluginFramework.IPlugin
    {
        public string PluginName { get; } = "PFC_Plugin";
        private Program Core;
        private TcpService service = new TcpService();
        private CancellationTokenSource cts = new CancellationTokenSource();
        Thread t1;
        public void onLoading()
        {
            Console.WriteLine(PluginName + "插件啟動中...");
            t1 = new Thread(() =>
            {
                
                service.Connecting = (client, e) => { };//有客户端正在连接
                service.Connected = (client, e) => { };//有客户端成功连接
                service.Disconnected = (client, e) => { };//有客户端断开连接
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
                            var value = Core.GetData(readModel).Result;
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
                            var value = Core.SetData(writeModel).Result;
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

                    //client.Logger.Info($"已從{client.ID}接收到信息：{mes}");

                    //client.Send(mes);//将收到的信息直接返回给发送方

                    //client.Send("id",mes);//将收到的信息返回给特定ID的客户端
                    /*
                    var ids = service.GetIDs();
                    foreach (var clientId in ids)//将收到的信息返回给在线的所有客户端。
                    {
                        if (clientId != client.ID)//不给自己发
                        {
                            //service.Send(clientId, mes);
                        }
                    }
                    */
                };
                //Console.WriteLine("------------");
                service.Setup(new TouchSocketConfig()//载入配置     
                    .SetListenIPHosts(new IPHost[] { new IPHost(5000) })//同时监听两个地址
                    .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                    {
                        a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                    })
                    .ConfigurePlugins(a =>
                    {
                        //a.Add();//此处可以添加插件
                    })
                    .SetDataHandlingAdapter(() => { return new TerminatorPackageAdapter("\r\n"); }))//配置终止字符适配器，以\r\n结尾。                                    
                    .Start();//启动                
                //Console.ReadKey();
            });
            t1.IsBackground = true;
            t1.Start();
        }
        public void onCloseing()
        {
            service.Stop();
            service.Dispose();
            service = null;            
            t1.Interrupt();
            t1 = null;
            Console.WriteLine(PluginName + "卸載中");
        }
        public void SetInstance(object dd)
        {
            Core = (Program)dd;
        }
    }
}
