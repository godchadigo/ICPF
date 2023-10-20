using TouchSocket.Core;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using PFCJsonRPC.Model;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace PFCJsonRPC
{
    
    public class JsonPFC
    {
        public bool isConnected = false;
        public WebSocketJsonRpcClient jsonRpcClient = new WebSocketJsonRpcClient();
        public event EventHandler<string> CommunicationStatusEvent;
        private object locker = new object();
        public JsonPFC()
        {

        }
        public void Connect()
        {
            ConnectServer();
        }
        public void Connect(string ipPort)
        {
            ConnectServer(ipPort);
        }

        private void ConnectServer(string ip = "ws://127.0.0.1:7707/ws")
        {
            try
            {
                jsonRpcClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(ip)
                .ConfigurePlugins(a =>
                {
                    a.UseReconnection(-1, true, 1000);                    
                })
                );//此url就是能连接到websocket的路径。

                jsonRpcClient.Connected += (client, e) =>
                {
                    Debug.WriteLine("上線");
                    CommunicationStatusEvent?.Invoke(this, "上線");
                    lock (locker)
                    {
                        isConnected = true;
                    }
                    return EasyTask.CompletedTask;
                };

                jsonRpcClient.Disconnected += (client, e) =>
                {
                    Debug.WriteLine("斷線");
                    CommunicationStatusEvent?.Invoke(this, "斷線");
                    lock (locker)
                        isConnected = false;
                    //RetryConnect();
                    return EasyTask.CompletedTask;
                };

                jsonRpcClient.Connect(1000);
                
            }
            catch (TimeoutException ex)
            {                
                Task.Run(async() =>
                {
                    await RetryConnect();
                });
                
            }                  
            //string result = jsonRpcClient.InvokeT<string>("TestJsonRpc", InvokeOption.WaitInvoke, "RRQM");
        }
        public async Task RetryConnect()
        {
            int retryCount = 0;
            int maxRetryCount = -1;
            int retryDelay = 100; // 1 秒

            while (!isConnected && retryCount < maxRetryCount || !isConnected && maxRetryCount == -1)
            {
                try
                {
                    
                    lock (locker)
                    {
                        jsonRpcClient.Connect();
                    }   
                    // 檢查是否連線成功
                    if (isConnected)
                    {

                        break;
                    }                                        
                }
                catch (Exception ex)
                {
                    // 處理連線異常
                }

                retryCount++;
                CommunicationStatusEvent?.Invoke(this, "重連" + retryCount);
                Debug.WriteLine("重連" + retryCount + "Status:" + isConnected);
                await Task.Delay(retryDelay);
            }

            if (!isConnected)
            {
                CommunicationStatusEvent?.Invoke(this, "重連連線超時");
                Console.WriteLine("連線超時");
                // 觸發通知事件
                // ...
                lock (locker)
                    isConnected = false;
            }
        }

        public WebSocketJsonRpcClient GetJsonPFC()
        {
            return jsonRpcClient;
        }
        public T SendObject<T>(string methodName , InvokeOption op , params object[] args)
        {            
            return jsonRpcClient.InvokeT<T>(methodName, op, args);
        }

        public async Task<T> SendObjectAsync<T>(string methodName, InvokeOption op, params object[] args)
        {
            return await jsonRpcClient.InvokeTAsync<T>(methodName, op, args);
        }

        //******//
        public OperationResult<List<string>> GetMachins()
        {
            if (!isConnected) return new OperationResult < List<string> >() { IsOk = false, Message = "FPC斷線!" };
            try
            {
                return SendObject<OperationResult<List<string>>>("GetMachins", InvokeOption.WaitInvoke);
            }
            catch (Exception ex)
            {
                return new OperationResult<List<string>>{ IsOk = false , Message = "FPC失敗" + ex.Message };
            }
        }
        public OperationResult<ConcurrentDictionary<string, QJTagData>> GetContainer(string deviceName)
        {
            if (!isConnected) return new OperationResult<ConcurrentDictionary<string, QJTagData>>() { IsOk = false , Message = "FPC斷線!"};
            try
            {
                return SendObject<OperationResult<ConcurrentDictionary<string, QJTagData>>>("GetContainer", InvokeOption.WaitInvoke, deviceName);
            }
            catch (Exception ex)
            {
                return new OperationResult<ConcurrentDictionary<string, QJTagData>>() { IsOk = false, Message = "FPC失敗!" + ex.Message };
            }            
        }
        public OperationResult<QJTagData> GetTag(string deviceName, string tagName)
        {
            if (!isConnected) return new OperationResult<QJTagData>() { IsOk = false, Message = "FPC斷線!" };
            try
            {
                
                return SendObject<OperationResult<QJTagData>>("GetTag", TouchSocket.Rpc.InvokeOption.WaitInvoke, "Modbus", "電壓");
            }
            catch (Exception ex)
            {
                return new OperationResult<QJTagData>() { IsOk = false, Message = "FPC失敗!" + ex.Message };
            }
        }
        public OperationResult<List<Tag>> GetTagList(string deviceName)
        {
            if (!isConnected) return new OperationResult<List<Tag>>() { IsOk = false, Message = "FPC斷線!" };
            try
            {

                return SendObject<OperationResult<List<Tag>>>("GetTagList", TouchSocket.Rpc.InvokeOption.WaitInvoke, deviceName);
            }
            catch (Exception ex)
            {
                return new OperationResult<List<Tag>>() { IsOk = false, Message = "FPC失敗!" + ex.Message };
            }
        }
        public OperationResult<List<QJTagData>> GetTagGroup(string deviceName, string groupName)
        {
            if (!isConnected) return new OperationResult<List<QJTagData>>() { IsOk = false, Message = "FPC斷線!" };
            try
            {

                return SendObject<OperationResult<List<QJTagData>>>("GetTagGroup", TouchSocket.Rpc.InvokeOption.WaitInvoke, deviceName , groupName);
            }
            catch (Exception ex)
            {
                return new OperationResult<List<QJTagData>>() { IsOk = false, Message = "FPC失敗!" + ex.Message };
            }
        }
    }
}