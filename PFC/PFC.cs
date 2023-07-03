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
                
            });
            ConnectWithRetry();
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
                config.SetRemoteIPHost(new IPHost("203.204.233.66:5000"))
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
        public OperationModel GetData(ReadDataModel model)
        {
            try
            {
                if (isConnected)
                {
                    var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(model , Newtonsoft.Json.Formatting.Indented);
                    tcpClient.Send(jsonStr);
                    return new OperationModel() { IsOk = true, Message = "通訊成功 : " };
                }
                else
                {
                    return new OperationModel() { IsOk = false, Message = "通訊失敗!" };
                }
            }
            catch (Exception ex)
            {
                return new OperationModel() { IsOk = false, Message = ex.Message };
            }
        }

    }

    #region QJProtocol
    public enum OperationType
    {
        Read = 1,
        Write = 2
    }
    [Obsolete("請使用ReadDataModel")]
    public class QJProtocolGetDataPacket
    {
        /// <summary>
        /// 讀取設備模組
        /// </summary>
        public ReadDataModel ReadPack { get; set; }
    }
    [Obsolete("請使用WriteDataModel")]
    public class QJProtocolSetDataPacket
    {
        /// <summary>
        /// 寫入設備模組
        /// </summary>
        public WriteDataModel WritePack { get; set; }
    }
    #endregion
    public class OperationModel
    {
        public bool IsOk { get; set; }
        public string Message { get; set; }
        public QJDataArray Data { get; set; }
    }
    public enum IRWDataOperation
    {
        Read = 1,
        Write = 2
    }
    public interface IRWData
    {
        /// <summary>
        /// 設備名稱
        /// 使用者需要指定定義好的設備
        /// </summary>
        string DeviceName { get; set; }
        /// <summary>
        /// 地址起點
        /// 讀取:讀取起點
        /// 寫入:寫入起點
        /// </summary>
        string Address { get; set; }
        IRWDataOperation iRWDataOperation { get; }
    }
    public class BaseDataModel : IRWData
    {
        public string DeviceName { get; set; }
        public string Address { get; set; }
        public IRWDataOperation iRWDataOperation { get; set; }

    }
    public class ReadDataModel : IRWData
    {
        public string DeviceName { get; set; }
        public string Address { get; set; }
        public ushort ReadLength { get; set; }
        public DataType DatasType { get; set; }
        public IRWDataOperation iRWDataOperation { get; } = IRWDataOperation.Read;

    }
    public class WriteDataModel : IRWData
    {
        public string DeviceName { get; set; }
        public string Address { get; set; }
        public object[] Datas { get; set; }
        public DataType DatasType { get; set; }
        public IRWDataOperation iRWDataOperation { get; } = IRWDataOperation.Write;
    }
    public enum DataType
    {
        Bool = 1,
        UInt16 = 2,
        Int16 = 3,
        UInt32 = 4,
        Int32 = 5,
        UInt64 = 6,
        Int64 = 7,
        Float = 8,
        Double = 9,
        String = 10,
    }
    /// <summary>
    /// QJData 特殊數據標記類
    /// QJData v1.基本資料類型標記
    /// </summary>
    public class QJData
    {
        public bool IsOk { get; set; }
        public object Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
    }
    public class QJDataArray
    {
        public bool IsOk { get; set; }
        public object[] Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
    }
    public class QJDataList
    {
        public bool IsOk { get; set; }
        public List<object> Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
    }
}