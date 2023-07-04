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
            Enterprise.Default.LicenceKey = "C22523CEC3DDDE276109E4B062CE6CA41FE01935603E304660E4735DF39FB39817668A4A48C1D6F5B1218E67972DF8CDD9768A208CB6A719C53BF9B7191AD60DD1AF10500BA9EF7080A837E517C8D38B";
        }
        private static TcpClient tcpClient = new TcpClient();
        public void Connect()
        {
            Task.Run(() => {
                
            });
            ConnectWithRetry();
        }
        private bool isConnected = false;
        private bool ReceviceFlag = false;
        private QJDataArray ReceviceBuffer;
        private string mes;
        private IWaitingClient<TcpClient> waitClient;

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
                    
                    mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                    Debug.WriteLine($"接收到信息：{mes}");
                    try
                    {
                        ReceviceFlag = true;
                        ReceviceBuffer = Newtonsoft.Json.JsonConvert.DeserializeObject<QJDataArray>(mes);                        
                    }
                    catch (Exception ex)
                    {
                        ReceviceFlag = false;
                    }
                    ReceviceFlag = false;
                };
                

                TouchSocketConfig config = new TouchSocketConfig();
                config.SetRemoteIPHost(new IPHost("127.0.0.1:5000"))
                    .UsePlugin()
                    .ConfigurePlugins(a =>
                    {
                        a.UseReconnection(-1, true, 100);
                    })
                    .SetDataHandlingAdapter(() => { return new TerminatorPackageAdapter("\r\n"); })//配置终止字符适配器，以\r\n结尾。
                    ;

                tcpClient.Setup(config);

                int retryCount = 0;

                while (!isConnected && retryCount < maxRetryCount || !isConnected && maxRetryCount == -1)
                {
                    try
                    {
                        await tcpClient.ConnectAsync();

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
        //QJDataArray
        public OperationModel GetData(ReadDataModel model)
        {
            try
            {
                if (isConnected)
                {
                    var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(model , Newtonsoft.Json.Formatting.None);
                    
                    //调用GetWaitingClient获取到IWaitingClient的对象。
                    waitClient = tcpClient.GetWaitingClient(new WaitingOptions()
                    {
                        AdapterFilter = AdapterFilter.AllAdapter,//表示发送和接收的数据都会经过适配器
                        BreakTrigger = true,//表示当连接断开时，会立即触发
                        ThrowBreakException = true//表示当连接断开时，是否触发异常
                    });
                    //然后使用SendThenReturn。
                    var packStr = Encoding.UTF8.GetBytes(jsonStr);
                    byte[] returnData = waitClient.SendThenReturn(packStr);
                    //tcpClient.Logger.Info($"收到回应消息：{Encoding.UTF8.GetString(returnData)}");                    
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<QJDataArray>(Encoding.UTF8.GetString(returnData));
                    return new OperationModel() { IsOk = true, DeviceName = data.DeviceName, Message = data.Message , Data = data };
                    
                    //return new OperationModel() { IsOk = false, Message = "通訊失敗!" };
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
       


        public OperationModel SetData(WriteDataModel model)
        {
            try
            {
                if (isConnected)
                {
                    var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None);
                    //调用GetWaitingClient获取到IWaitingClient的对象。
                    waitClient = tcpClient.GetWaitingClient(new WaitingOptions()
                    {
                        AdapterFilter = AdapterFilter.AllAdapter,//表示发送和接收的数据都会经过适配器
                        BreakTrigger = true,//表示当连接断开时，会立即触发
                        ThrowBreakException = true//表示当连接断开时，是否触发异常
                    });
                    //然后使用SendThenReturn。
                    var packStr = Encoding.UTF8.GetBytes(jsonStr);
                    byte[] returnData = waitClient.SendThenReturn(packStr);
                    //tcpClient.Logger.Info($"收到回应消息：{Encoding.UTF8.GetString(returnData)}");                    
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<QJDataArray>(Encoding.UTF8.GetString(returnData));
                    return new OperationModel() { IsOk = true, DeviceName = data.DeviceName, Message = data.Message, Data = new QJDataArray() { Data = model.Datas} };

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
        public string DeviceName { get; set; }
        public string Message { get; set; }
        public QJDataArray Data { get; set; }
        public override string ToString()
        {
            return $"請求結果 : {IsOk} ， 數據 : {Data}";
        }
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
        public override string ToString()
        {
            string strRes = string.Empty;
            if (Datas != null)
            {
                if (Datas.Length > 0)
                    foreach (var str in Datas)
                    {
                        strRes += str.ToString() + " ";
                    }                
            }
            else
            {
                strRes = "";
            }

            return string.Format("寫入設備名稱:{0}，寫入地址:{1}，寫入數據:{2}，寫入類型{3}" , DeviceName , Address , strRes , DatasType.ToString() );
        }

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
        public string DeviceName { get; set; }
        public object[] Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
        public override string ToString()
        {
            string strRes = string.Empty;
            if (Data != null)
            {
                if (IsOk && Data.Length > 0)
                    foreach (var str in Data)
                    {
                        strRes += str.ToString() + " ";
                    }
            }
            else
            {
                strRes = "";
            }
            
            return (strRes);
        }
    }
    public class QJDataList
    {
        public bool IsOk { get; set; }
        public List<object> Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
    }
}