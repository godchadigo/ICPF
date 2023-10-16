using System.Net;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Rpc.WebApi;
using TouchSocket.Sockets;
using ICPFCore;
using System.Collections.Concurrent;
using System.Linq;
using Plugin.PFCClient.Model;

namespace PluginPFCClient
{
    public class Main : ICPFCore.PluginBase
    {
        public override string PluginName { get; set; } = "PFC_Plugin";

        private TcpService service = new TcpService();
        private CancellationTokenSource cts = new CancellationTokenSource();
        Thread t1;
        Thread t2;

        private ConcurrentQueue<IRWData> dataPacketList = new ConcurrentQueue<IRWData>();

        private SocketClient sc;

        public override void onLoading()
        {
            base.onLoading();
            
            t1 = new Thread(() =>
            {

                service.Connecting = (client, e) => { };//有客户端正在連接
                service.Connected = (client, e) => {
                    client.Logger.Info(client.ID + "加入了會議!");
                };//有客户端成功連接
                service.Disconnected = (client, e) => { };//有客户端段開連接
                service.Received = async (client, byteBlock, requestInfo) =>
                {
                    sc = client;
                    //从客户端收到信息
                    string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);

                    if (false)
                    {
                        client.Logger.Info("###################Start#################\r\n");
                        client.Logger.Info(mes + "\r\n");
                        client.Logger.Info("####################End##################\r\n");
                    }

                    try
                    {
                        BaseDataModel packRes = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseDataModel>(mes);

                        if (packRes.iRWDataOperation == IRWDataOperation.ReadData)
                        {

                            var readModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ReadDataModel>(mes);
                            var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(readModel);
                            client.Logger.Info("read: " + jsonStr);
                            //dataPacketList.Enqueue(readModel);
                            var result = new BaseDataModel();

                            var a = await GetData(readModel);
                            Console.WriteLine("a" + a.DeviceName + a.IsOk + a.Data.Length + a.Message + a.Uuid);
                            var jsonStra = Newtonsoft.Json.JsonConvert.SerializeObject(a);
                            await sc.SendAsync(jsonStra);


                        }

                        if (packRes.iRWDataOperation == IRWDataOperation.WriteData)
                        {

                            var writeModel = Newtonsoft.Json.JsonConvert.DeserializeObject<WriteDataModel>(mes);
                            //dataPacketList.Enqueue(writeModel);
                            var b = await SetData(writeModel);
                            Console.WriteLine("b" + b.Message + b.Uuid);
                            var jsonStrb = Newtonsoft.Json.JsonConvert.SerializeObject(b);
                            await sc.SendAsync(jsonStrb);

                        }

                        if (packRes.iRWDataOperation == IRWDataOperation.Command)
                        {
                            if (packRes.iRWCommand == IRWDataCommand.GetMacines)
                            {
                                var machinResult = await Core.GetMachins();
                                machinResult.Uuid = packRes.Uuid;
                                var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(machinResult);
                                Console.WriteLine("GetMachins: " + jsonStr);
                                await sc.SendAsync(jsonStr);
                            }

                            if (packRes.iRWCommand == IRWDataCommand.GetTagName)
                            {
                                var machinResult = await Core.GetTag(packRes.DeviceName, packRes.Address);
                                machinResult.Uuid = packRes.Uuid;
                                var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(machinResult);
                                Console.WriteLine("GetTag " + jsonStr);
                                await sc.SendAsync(jsonStr);
                            }
                            if (packRes.iRWCommand == IRWDataCommand.GetTagGroup)
                            {
                                var machinResult = await Core.GetTagGroup(packRes.DeviceName, packRes.Address);
                                machinResult.Uuid = packRes.Uuid;
                                Console.Write("UUid is :" + machinResult.Uuid);
                                var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(machinResult);
                                Console.WriteLine("GetTagGroup " + jsonStr);
                                await sc.SendAsync(jsonStr);
                            }

                            if (packRes.iRWCommand == IRWDataCommand.GetContainer)
                            {
                                var result = await Core.GetDeviceContainer(packRes.Address);                                
                                result.Uuid = packRes.Uuid;
                                var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                                
                                Console.WriteLine("GetContainer " + jsonStr);
                                sc.Send(jsonStr);
                            }
                        }
                    }
                    catch (Exception ex) { }

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

            t2 = new Thread(async () => {
                while (true)
                {
                    if (dataPacketList.TryDequeue(out IRWData model))
                    {
                        var result = new BaseDataModel();
                        if (model == null) continue;
                        if (model is ReadDataModel readModel)
                        {
                            var a = await GetData(readModel);
                            Console.WriteLine("a" + a.DeviceName + a.IsOk + a.Data.Length + a.Message + a.Uuid);
                            var jsonStra = Newtonsoft.Json.JsonConvert.SerializeObject(a);
                            await sc.SendAsync(jsonStra);
                        }
                        if (model is WriteDataModel writeModel)
                        {
                            var b = await SetData(writeModel);
                            Console.WriteLine("b" + b.Message + b.Uuid);
                            var jsonStrb = Newtonsoft.Json.JsonConvert.SerializeObject(b);
                            await sc.SendAsync(jsonStrb);
                        }
                    }
                    Thread.Sleep(0);
                }
            });
            t2.IsBackground = true;
            //t2.Start();

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
        private List<ContainerModel> ContainerBuffer = new List<ContainerModel>();
        private readonly object locker = new object();
        
    }
}
