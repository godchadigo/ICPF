using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace PFC
{
    public class PFC : IPFC
    {
        public PFC()
        {
            Enterprise.Default.LicenceKey = "C22523CEC3DDDE276109E4B062CE6CA41FE01935603E304660E4735DF39FB39817668A4A48C1D6F5B1218E67972DF8CDD9768A208CB6A719C53BF9B7191AD60DD1AF10500BA9EF7080A837E517C8D38B";
        }
        private static TcpClient tcpClient = new TcpClient();
        public void Connect()
        {           
            ConnectWithRetry();
            //ContainerLoop();
        }
        public void Connect(string ip_port)
        {
            IpAddressPort = ip_port;
            ConnectWithRetry();
            //ContainerLoop();
        }
        private bool isConnected = false;
        private bool ReceviceFlag = false;
        private string ReceviceBufferString;
        private string mes;
        private IWaitingClient<TcpClient> waitClient;
        public event EventHandler<string> CommunicationStatusEvent;
        private string IpAddressPort { get; set; } = "45.32.56.98:5000";


        private async void ConnectWithRetry()
        {            
            try
            {
                
                tcpClient.Connecting += (client, e) =>
                {
                    //Debug.WriteLine("recon?");
                    //isConnected = true;
                };
                tcpClient.Connected += (client, e) =>
                {
                    Debug.WriteLine("上線");
                    CommunicationStatusEvent?.Invoke(this, "上線");
                    isConnected = true;
                };

                tcpClient.Disconnected += (client, e) =>
                {
                    Debug.WriteLine("斷線");
                    CommunicationStatusEvent?.Invoke(this, "斷線");
                    isConnected = false;
                    RetryConnect();
                };

                tcpClient.Received += (client, byteBlock, requestInfo) =>
                {
                    
                    mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                    Debug.WriteLine($"接收到信息：{mes}");
                    try
                    {
                        
                        ReceviceFlag = true;
                        if (mes.Length != 0)
                        {
                            ReceviceBufferString = mes;
                        }
                        else
                        {
                            ReceviceFlag = false;
                        }                            
                    }
                    catch (Exception ex)
                    {
                        ReceviceFlag = false;
                    }
                    ReceviceFlag = false;
                };
                

                TouchSocketConfig config = new TouchSocketConfig();
                config.SetRemoteIPHost(new IPHost(IpAddressPort))
                    .UsePlugin()
                    .ConfigurePlugins(a =>
                    {
                        a.UseReconnection(-1, true, 100);
                    })
                    .SetDataHandlingAdapter(() => { return new TerminatorPackageAdapter("\r\n"); })//配置终止字符适配器，以\r\n结尾。
                    ;

                tcpClient.Setup(config);
                await RetryConnect();




            }
            catch (Exception ex)
            {
                // 處理異常
                isConnected = false;
            }            
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
                CommunicationStatusEvent?.Invoke(this, "重連" + retryCount);
                Debug.WriteLine("重連" + retryCount);
                await Task.Delay(retryDelay);
            }

            if (!isConnected)
            {
                CommunicationStatusEvent?.Invoke(this, "重連連線超時");
                Console.WriteLine("連線超時");
                // 觸發通知事件
                // ...
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
        public async Task<OperationModel> GetData(ReadDataModel model)
        {
            try
            {
                if (isConnected)
                {
                    BaseDataModel readModel = new BaseDataModel();
                    readModel.Uuid = Guid.NewGuid().ToString();
                    readModel.Address = model.Address;
                    readModel.DeviceName = model.DeviceName;
                    readModel.iRWDataOperation = IRWDataOperation.Read;
                                        
                    var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None);
                    await tcpClient.SendAsync(jsonStr);

                    // 創建一個 Stopwatch
                    Stopwatch stopwatch = new Stopwatch();

                    // 開始計時
                    stopwatch.Start();

                    while (true)
                    {
                        // 如果經過的時間超過了你設定的超時時間（例如5秒），則退出循環
                        if (stopwatch.ElapsedMilliseconds > 1000) // 5000毫秒 = 5秒
                        {
                            break;
                        }

                        if (ReceviceBufferString == null) continue;
                        var ReceviceBuffer = Newtonsoft.Json.JsonConvert.DeserializeObject<QJDataArray>(ReceviceBufferString);
                        if (ReceviceBuffer.Uuid == model.Uuid)
                        {
                            if (ReceviceBuffer.Data == null) continue;
                            if (ReceviceBuffer.Data.Length != model.ReadLength)
                            {
                                return new OperationModel() { IsOk = false, DeviceName = ReceviceBuffer.DeviceName, Message = "讀取設備端的數據異常(設備端讀出的長度與使用者設定的不一致)", Data = (QJDataArray)ReceviceBuffer };
                            }
                            else
                            {
                                return new OperationModel() { IsOk = ReceviceBuffer.IsOk, DeviceName = ReceviceBuffer.DeviceName, Message = ReceviceBuffer.Message, Data = (QJDataArray)ReceviceBuffer };
                            }
                        }
                            
                    }

                    return new OperationModel() { IsOk = false , Message = "接收超時!"};
                    /*
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
                    return new OperationModel() { IsOk = data.IsOk, DeviceName = data.DeviceName, Message = data.Message , Data = data };
                    
                    //return new OperationModel() { IsOk = false, Message = "通訊失敗!" };
                    */
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
       
        public async Task<OperationModel> SetData(WriteDataModel model)
        {
            try
            {
                if (isConnected)
                {
                    BaseDataModel writeModel = new BaseDataModel();
                    writeModel.Uuid = Guid.NewGuid().ToString();
                    writeModel.Address = model.Address;
                    writeModel.DeviceName = model.DeviceName;
                    writeModel.iRWDataOperation = IRWDataOperation.Read;
                    
                    var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None);
                    await tcpClient.SendAsync(jsonStr);

                    // 創建一個 Stopwatch
                    Stopwatch stopwatch = new Stopwatch();

                    // 開始計時
                    stopwatch.Start();

                    while (true)
                    {
                        // 如果經過的時間超過了你設定的超時時間（例如5秒），則退出循環
                        if (stopwatch.ElapsedMilliseconds > 50) // 5000毫秒 = 5秒
                        {
                            break;
                        }
                        if (ReceviceBufferString == null) continue;
                        var ReceviceBuffer = Newtonsoft.Json.JsonConvert.DeserializeObject<QJDataArray>(ReceviceBufferString);
                        if (ReceviceBuffer.Uuid == model.Uuid)
                            return new OperationModel() { IsOk = ReceviceBuffer.IsOk, DeviceName = ReceviceBuffer.DeviceName, Message = ReceviceBuffer.Message, Data = (QJDataArray)ReceviceBuffer };
                    }

                    return new OperationModel() { IsOk = false, Message = "接收超時!" };

                    /*
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
                    */
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

        public async Task<OperationResult<QJTagData>> GetTag(string deviceName, string tagName)
        {
            try
            {
                if (isConnected)
                {
                    BaseDataModel readModel = new BaseDataModel();
                    readModel.Uuid = Guid.NewGuid().ToString();
                    readModel.Address = tagName;
                    readModel.DeviceName = deviceName;
                    readModel.iRWDataOperation = IRWDataOperation.Command;
                    readModel.iRWCommand = IRWDataCommand.GetTagName;

                    var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(readModel, Newtonsoft.Json.Formatting.None);
                    await tcpClient.SendAsync(jsonStr);

                    // 創建一個 Stopwatch
                    Stopwatch stopwatch = new Stopwatch();

                    // 開始計時
                    stopwatch.Start();

                    while (true)
                    {
                        // 如果經過的時間超過了你設定的超時時間（例如5秒），則退出循環
                        if (stopwatch.ElapsedMilliseconds > 1000) // 5000毫秒 = 5秒
                        {
                            break;
                        }

                        if (ReceviceBufferString == null) continue;
                        if (ReceviceBufferString == "") continue;
                        var ReceviceBuffer = Newtonsoft.Json.JsonConvert.DeserializeObject<OperationResult<QJTagData>>(ReceviceBufferString);
                        if (ReceviceBuffer.Uuid == readModel.Uuid)
                        {
                            if (ReceviceBuffer.Data == null) continue;
                            ReceviceBufferString = "";
                            return ReceviceBuffer;
                        }
                    }

                    return new OperationResult<QJTagData>() { IsOk = false, Message = "接收超時!" };
                  
                }
                else
                {
                    return new OperationResult<QJTagData>() { IsOk = false, Message = "通訊失敗!" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<QJTagData>() { IsOk = false, Message = ex.Message };
            }
        }
        public async Task<OperationResult<List<QJTagData>>> GetTagGroup(string deviceName, string groupName)
        {
            try
            {
                if (isConnected)
                {
                    BaseDataModel readModel = new BaseDataModel();
                    readModel.Uuid = Guid.NewGuid().ToString();
                    readModel.Address = groupName;
                    readModel.DeviceName = deviceName;
                    readModel.iRWDataOperation = IRWDataOperation.Command;
                    readModel.iRWCommand = IRWDataCommand.GetTagGroup;

                    var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(readModel, Newtonsoft.Json.Formatting.None);
                    await tcpClient.SendAsync(jsonStr);

                    // 創建一個 Stopwatch
                    Stopwatch stopwatch = new Stopwatch();

                    // 開始計時
                    stopwatch.Start();

                    while (true)
                    {
                        // 如果經過的時間超過了你設定的超時時間（例如5秒），則退出循環
                        if (stopwatch.ElapsedMilliseconds > 1000) // 5000毫秒 = 5秒
                        {
                            break;
                        }

                        if (ReceviceBufferString == "") continue;
                        var ReceviceBuffer = Newtonsoft.Json.JsonConvert.DeserializeObject<OperationResult<List<QJTagData>>>(ReceviceBufferString);
                        if (ReceviceBuffer?.Uuid == readModel.Uuid)
                        {                                                                             
                            ReceviceBufferString = String.Empty;
                            return ReceviceBuffer;
                        }
                    }
                    return new OperationResult<List<QJTagData>> { IsOk = false, Message = "接收超時!" };
                }
                else
                {
                    return new OperationResult<List<QJTagData>>{ IsOk = false, Message = "通訊失敗!" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<List<QJTagData>> { IsOk = false, Message = ex.Message };
            }
        }

        public async Task<OperationResult<List<string>>> GetMachins()
        {
            try
            {
                if (isConnected)
                {
                    BaseDataModel model = new BaseDataModel();
                    model.Uuid = Guid.NewGuid().ToString();
                    model.iRWDataOperation = IRWDataOperation.Command;
                    model.iRWCommand = IRWDataCommand.GetMacines;

                    var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None);
                    await tcpClient.SendAsync(jsonStr);

                    // 創建一個 Stopwatch
                    Stopwatch stopwatch = new Stopwatch();

                    // 開始計時
                    stopwatch.Start();

                    while (true)
                    {
                        // 如果經過的時間超過了你設定的超時時間（例如5秒），則退出循環
                        if (stopwatch.ElapsedMilliseconds > 2000) // 5000毫秒 = 5秒
                        {
                            break;
                        }
                        
                        if (ReceviceBufferString == null) continue;
                        if (ReceviceBufferString == "") continue;
                        var ReceviceBuffer = Newtonsoft.Json.JsonConvert.DeserializeObject<OperationResult<List<string>>>(ReceviceBufferString);
                        if (ReceviceBuffer.Uuid == model.Uuid)
                        {
                            ReceviceBufferString = "";
                            return ReceviceBuffer;
                        }                            
                    }
                    return new OperationResult<List<string>> { IsOk = false, Message = "接收超時!" };                    
                }
                else
                {
                    return new OperationResult<List<string>> { IsOk = false, Message = "通訊失敗!" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<List<string>> { IsOk = false, Message = ex.Message };
            }
        }
        public async Task<OperationResult<ConcurrentDictionary<string, QJTagData>>> GetContainer(string deviceName)
        {
            try
            {
                if (isConnected)
                {
                    BaseDataModel model = new BaseDataModel();
                    model.Uuid = Guid.NewGuid().ToString();
                    model.iRWDataOperation = IRWDataOperation.Command;
                    model.iRWCommand = IRWDataCommand.GetContainer;
                    model.Address = deviceName;

                    Console.WriteLine("GetContainer: " + model.Uuid);
                    var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None);
                    await tcpClient.SendAsync(jsonStr);

                    // 創建一個 Stopwatch
                    Stopwatch stopwatch = new Stopwatch();

                    // 開始計時
                    stopwatch.Start();

                    while (true)
                    {
                        // 如果經過的時間超過了你設定的超時時間（例如5秒），則退出循環
                        if (stopwatch.ElapsedMilliseconds > 10000) // 5000毫秒 = 5秒
                        {
                            break;
                        }
                        if (ReceviceBufferString == "") continue;
                        var ReceviceBuffer = Newtonsoft.Json.JsonConvert.DeserializeObject<OperationResult<ConcurrentDictionary<string, QJTagData>>>(ReceviceBufferString);
                        if (ReceviceBuffer.Uuid == model.Uuid)
                        {
                            ReceviceBufferString = String.Empty;
                            return ReceviceBuffer;
                        }
                            
                    }

                    return new OperationResult<ConcurrentDictionary<string, QJTagData>> { IsOk = false, Message = "接收超時!" };
                }
                else
                {
                    return new OperationResult<ConcurrentDictionary<string, QJTagData>> { IsOk = false, Message = "通訊失敗!" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<ConcurrentDictionary<string, QJTagData>> { IsOk = false, Message = ex.Message };
            }
        }
        public OperationModel AddController(string  deviceName , int commInterface , int CommProtocol , string ip , int port)
        {

            return new OperationModel() { IsOk = false };
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
    public class OperationModel<T>
    {
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public override string ToString()
        {
            return $"請求結果 : {IsOk} ， 數據 : {Data}";
        }
    }


    public class OperationTagModel
    {
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public string Message { get; set; }
        public QJTagDataArray Data { get; set; }
        public override string ToString()
        {
            return $"請求結果 : {IsOk} ， 數據 : {Data}";
        }
    }
    public class OperationTagGroupModel
    {
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public string Message { get; set; }
        public QJTagGroupDataArray Data { get; set; }
        public override string ToString()
        {
            return $"請求結果 : {IsOk} ， 數據 : {Data}";
        }
    }
    public class ContainerModel
    {
        public string Uuid { get; set; }
        public string DeviceName { get; set; }
        public string TagGroup { get; set; }
        public string TagName { get; set; }
        public QJDataArray Data { get; set; }

    }
    public class ContainerModelPacket
    {
        public string Uuid { get; set; }
        public List<ContainerModel> Container { get; set; }
    }
    public class Tag
    {
        public string GroupName { get; set; }
        public string TagName { get; set; }
        public bool IsOk { get; set; } = false;
        public string Address { get; set; }
        public DataType DataType { get; set; }
        public ushort Length { get; } = 1;
    }

    public enum IRWDataOperation
    {
        Read = 1,
        Write = 2,
        Command = 3,
    }
    public enum IRWDataCommand
    {
        GetMacines = 1,
        GetTagName = 2,
        GetTagGroup = 3,
        GetContainer = 4,
    }
    public interface IRWData
    {
        string Uuid { get; set; }
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
        public string Uuid { get; set; }
        public string DeviceName { get; set; }
        public string Address { get; set; }
        public IRWDataOperation iRWDataOperation { get; set; }
        public IRWDataCommand iRWCommand { get; set; }

    }
    public class ReadDataModel : IRWData
    {
        public string Uuid { get; set; }
        public string DeviceName { get; set; }
        public string Address { get; set; }
        public ushort ReadLength { get; set; }
        public DataType DatasType { get; set; }
        public IRWDataOperation iRWDataOperation { get; } = IRWDataOperation.Read;

    }
    public class WriteDataModel : IRWData
    {
        public string Uuid { get; set; }
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
        public string Uuid { get; set; }
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public string Address { get; set; }
        public object Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
    }
    public class QJTagData : QJData
    {
        public string GroupName { get; set; }
        public string TagName { get; set; }
    }
    public interface IQJData
    {
        public string Uuid { get; set; }
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public object[] Data { get; set; }
        public string Message { get; set; }
    }
    public class QJDataArray:IQJData
    {
        public string Uuid { get; set; }
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
    public class QJTagDataArray : QJDataArray , IQJData
    {
        public string TagName { get; set; }
    }
    public class QJTagGroupDataArray : QJDataArray , IQJData
    {
        public string TagName { get; set; }
        public Dictionary<string, QJDataArray> TagData { get; set; }
    }
    public class QJDataList
    {
        public bool IsOk { get; set; }
        public List<object> Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
    }
    public class OperationResult<T>
    {
        public string Uuid { get; set; }
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}