using System.Diagnostics;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace PFC
{
    public class PFC
    {
        public PFC()
        {

        }
        private static TcpClient tcpClient = new TcpClient();
        public void Connect()
        {
            Task.Run(() => {
                ConnectWithRetry();
            });
            
        }
        private bool isConnected = false;
        private async void ConnectWithRetry()
        {
            
            int maxRetryCount = -1;
            int retryDelay = 100; // 1 秒

            try
            {
                tcpClient.Connected += (client, e) =>
                {
                    Debug.WriteLine("上線");
                    isConnected = true;
                };

                tcpClient.Disconnected += (client, e) =>
                {
                    Debug.WriteLine("斷線");
                };

                tcpClient.Received += (client, byteBlock, requestInfo) =>
                {
                    string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                    Debug.WriteLine($"接收到信息：{mes}");
                };

                TouchSocketConfig config = new TouchSocketConfig();
                config.SetRemoteIPHost(new IPHost("127.0.0.1:5000"))
                    .UsePlugin()
                    .ConfigurePlugins(a =>
                    {
                        a.UseReconnection(-1, true, 100);
                    });

                tcpClient.Setup(config);

                int retryCount = 0;

                while (!isConnected && retryCount < maxRetryCount || !isConnected && maxRetryCount == -1)
                {
                    try
                    {
                        await tcpClient.ConnectAsync();

                        // 檢查是否連線成功
                        if (isConnected)
                            break;
                    }
                    catch (Exception ex)
                    {
                        // 處理連線異常
                    }

                    retryCount++;
                    Debug.WriteLine("重連" + retryCount);
                    await Task.Delay(retryDelay);
                }

                if (!isConnected)
                {
                    Console.WriteLine("連線超時");
                    // 觸發通知事件
                    // ...
                    isConnected = false;
                }
            }
            catch (Exception ex)
            {
                // 處理異常
                isConnected = false;
            }
        }

        public OperationModel Send(string cmd)
        {
            try
            {
                if (isConnected)
                {
                    tcpClient.Send(cmd);
                    return new OperationModel() { IsOk = true, Message = "通訊成功 : " };
                }
                else
                {
                    return new OperationModel() { IsOk = false, Message = "通訊失敗!" };
                }                    
            }
            catch (Exception ex) {
                return new OperationModel() { IsOk = false , Message = ex.Message };
            }            
        }
        public OperationModel GetData()
        {

        }

    }
    public class OperationModel
    {
        public bool IsOk { get; set; }
        public string Message { get; set; }
        public QJDataArray Data { get; set; }
    }

    public class QJDataArray
    {
        public bool IsOk { get; set; }
        public object[] Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
    }
}