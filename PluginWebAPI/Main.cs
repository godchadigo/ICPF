﻿using ConsolePluginTest;
using System.Net;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Rpc.WebApi;
using TouchSocket.Sockets;

namespace PluginWebAPI
{
    public class Main : PluginFramework.IPlugin
    {
        private Program Core;
        public void onLoading()
        {
            Task.Run(() =>
            {
                TcpService service = new TcpService();
                service.Connecting = (client, e) => { };//有客户端正在连接
                service.Connected = (client, e) => { };//有客户端成功连接
                service.Disconnected = (client, e) => { };//有客户端断开连接
                service.Received = (client, byteBlock, requestInfo) =>
                {
                    //从客户端收到信息
                    string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                    var packRes = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseDataModel>(mes);
                    client.Logger.Info(packRes.iRWDataOperation.ToString());
                    if (packRes.iRWDataOperation == IRWDataOperation.Read)
                    {
                        try
                        {
                            var readModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ReadDataModel>(mes);
                            //client.Logger.Info($"地址:{readModel.Address}");
                            var value = Core.GetData(readModel).Result;
                            var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                            client.Send(jsonStr);
                            client.Logger.Info(jsonStr);
                        }
                        catch (Exception ex) { }
                    }
                    if (packRes.iRWDataOperation == IRWDataOperation.Write)
                    {
                        try
                        {
                            var writeModel = Newtonsoft.Json.JsonConvert.DeserializeObject<WriteDataModel>(mes);
                            //client.Logger.Info($"地址:{writeModel.Address}");
                        }
                        catch (Exception ex) { }
                    }

                    //client.Logger.Info($"已從{client.ID}接收到信息：{mes}");

                    //client.Send(mes);//将收到的信息直接返回给发送方

                    //client.Send("id",mes);//将收到的信息返回给特定ID的客户端

                    var ids = service.GetIDs();
                    foreach (var clientId in ids)//将收到的信息返回给在线的所有客户端。
                    {
                        if (clientId != client.ID)//不给自己发
                        {
                            //service.Send(clientId, mes);
                        }
                    }
                };

                service.Setup(new TouchSocketConfig()//载入配置     
                    .SetListenIPHosts(new IPHost[] { new IPHost(5000) })//同时监听两个地址
                    .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                    {
                        a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                    })
                    .ConfigurePlugins(a =>
                    {
                        //a.Add();//此处可以添加插件
                    }))
                    .Start();//启动
                //Console.ReadKey();
            });
        }
        public void SetInstance(object dd)
        {
            Core = (Program)dd;
        }
    }
}
