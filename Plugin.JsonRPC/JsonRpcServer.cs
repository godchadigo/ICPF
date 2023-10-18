using Newtonsoft.Json.Linq;
using TouchSocket.Http;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using ICPFCore;
using System.Collections.Concurrent;

namespace Plugin.JsonRPC
{
    public class JsonRpcServer : RpcServer
    {
        private ICPFCore.Program Core;
        public JsonRpcServer(ICPFCore.Program _core)
        {
            Core = _core;
        }
        /// <summary>
        /// 使用调用上下文。
        /// 可以从上下文获取调用的SocketClient。从而获得IP和Port等相关信息。
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        [JsonRpc(MethodFlags = MethodFlags.IncludeCallContext, MethodInvoke = true)]
        public string TestGetContext(ICallContext callContext, string str)
        {
            if (callContext.Caller is IHttpSocketClient socketClient)
            {
                if (socketClient.Protocol == Protocol.WebSocket)
                {
                    Console.WriteLine("WebSocket请求");
                    var client = callContext.Caller as IHttpSocketClient;
                    var ip = client.IP;
                    var port = client.Port;
                    Console.WriteLine($"WebSocket请求{ip}:{port}");
                }
                else
                {
                    Console.WriteLine("HTTP请求");
                    var client = callContext.Caller as IHttpSocketClient;
                    var ip = client.IP;
                    var port = client.Port;
                    Console.WriteLine($"HTTP请求{ip}:{port}");
                }
            }
            else if (callContext.Caller is ISocketClient)
            {
                Console.WriteLine("Tcp请求");
                var client = callContext.Caller as ISocketClient;
                var ip = client.IP;
                var port = client.Port;
                Console.WriteLine($"Tcp请求{ip}:{port}");
            }
            return "RRQM" + str;
        }

        [JsonRpc(MethodInvoke = true)]
        public JObject TestJObject(JObject obj)
        {
            return obj;
        }

        [JsonRpc(MethodInvoke = true)]
        public string TestJsonRpc(string str)
        {
            return "RRQM" + str;
        }
        [JsonRpc(MethodInvoke = true)]
        public OperationResult<List<string>> GetMachins()
        {
            var res = Core.GetMachins();
            return res;
        }

        [JsonRpc(MethodInvoke = true)]
        public OperationResult<ConcurrentDictionary<string, QJTagData>> GetContainer(string deviceName)
        {
            var res = Core.GetDeviceContainer(deviceName);
            return res;
        }
        [JsonRpc(MethodInvoke = true)]
        public OperationResult<QJTagData> GetTag(string deviceName, string tagName)
        {
            var res = Core.GetTag(deviceName , tagName);
            return res;
        }
    }
}