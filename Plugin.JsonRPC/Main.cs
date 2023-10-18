using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace Plugin.JsonRPC
{
    public class Main : ICPFCore.PluginBase
    {
        private HttpService service = new HttpService();
        public override string PluginName { get; set; } = "JsonRPC_Plugin";
        public override void onLoading()
        {
            base.onLoading();
            Console.WriteLine("JsonRPC Loading...");
            StartService();
        }
        public override void onCloseing()
        {
            base.onCloseing();
            service.Stop();
            service.Dispose();
            service = new HttpService();
            Console.WriteLine(PluginName + "關閉成功!");
        }
        public void StartService()
        {
            
            //注入Core服務
            var jsonRpcServer = new JsonRpcServer(Core);
            
            service.Setup(new TouchSocketConfig()
                 .SetListenIPHosts(7707)
                 .ConfigurePlugins(a =>
                 {
                     a.UseWebSocket()
                     .SetWSUrl("/ws");

                     a.UseWebSocketJsonRpc()
                     .SetAllowJsonRpc((socketClient, context) =>
                     {
                         //此处的作用是，通过连接的一些信息判断该ws是否执行JsonRpc。
                         //当然除了此处可以设置外，也可以通过socketClient.SetJsonRpc(true)直接设置。
                         return true;
                     })
                     .ConfigureRpcStore(store =>
                     {
                         store.RegisterServer(jsonRpcServer);
                     });
                 }))
                .Start();
        }
        public ICPFCore.Program GetInstance()
        {
            return Core;
        }
    }
}
