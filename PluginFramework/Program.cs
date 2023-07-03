using HslCommunication;
using HslCommunication.Core.Net;
using LibaryShare;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using Newtonsoft.Json;
using PluginFramework;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reflection;
using System.Xml.Linq;

namespace ConsolePluginTest
{

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
    public enum CommunicationInterface
    {
        Serial = 1,
        Ethernet = 2,        
    }
    public enum CommunicationProtocol
    {
        KvHost = 1,
        McProtocol_Tcp = 2,
        Modbus_Tcp = 3,
        Vigor_Tcp = 4,
    }
    public interface IDeviceConfig
    {
        string DeviceName { get; set; }
        CommunicationInterface CommunicationInterface { get; }
        CommunicationProtocol CommunicationProtocol { get; set; }        
    }
    /// <summary>
    /// 基類通訊模型
    /// </summary>
    public class BaseDeviceConfigModel : IDeviceConfig
    {
        public string DeviceName { get; set; }
        public CommunicationInterface CommunicationInterface { get; set; }
        public CommunicationProtocol CommunicationProtocol { get; set; }

    }

    /// <summary>
    /// 網路型通訊模型
    /// </summary>
    public class EthernetDeviceConfigModel : IDeviceConfig
    {
        public string DeviceName { get; set; }
        public CommunicationInterface CommunicationInterface { get; } = CommunicationInterface.Ethernet;
        public CommunicationProtocol CommunicationProtocol { get; set; }
        public string IP { get;set; }
        public int Port { get;set; }

    }
    /// <summary>
    /// 串口暫不開發
    /// </summary>
    public class SerialDeviceConfigModel : IDeviceConfig
    {
        public string DeviceName { get; set; }
        public CommunicationInterface CommunicationInterface { get; set; } = CommunicationInterface.Serial;
        public CommunicationProtocol CommunicationProtocol { get; set; }
        public string ComName { get; set; }
        
    }
    public class Program 
    {
        public static event EventHandler<EventArgs> ProgramCreated;
        private static ConcurrentDictionary<string , NetworkDeviceBase> NetDeviceList = new ConcurrentDictionary<string , NetworkDeviceBase>();
        private static Program p;
        private static List<IPlugin> plugins = new List<IPlugin>();
        public static string Test { get; set; } = "test123456";

        public static Program GetInstance() 
        {
            return p;
        }
        
        public void Start()
        {
            ConsolePluginTest.Program.Main(null);
        }
        static void Main(string[] args)
        {
            
            //***** +測試空間+ *****//
            EthernetDeviceConfigModel ethModel = new EthernetDeviceConfigModel();
            ethModel.DeviceName = "Keyence8500_1";
            ethModel.CommunicationProtocol = CommunicationProtocol.KvHost;
            ethModel.IP = "192.168.0.10";
            ethModel.Port = 8501;
            string jsonString = JsonConvert.SerializeObject(ethModel , Formatting.Indented);
            var dirPath = System.IO.Directory.GetCurrentDirectory();
            // 指定本地文件路径
            string filePath = $"{dirPath}/DeviceConfig/Keyence.json";
            //File.WriteAllText(filePath, jsonString);
            //***** -測試空間- *****//
            //return;
            CommunicationTask();
            
            #region 插件反射載入
            p = new Program();            
            List<string> pluginpath = p.FindPlugin();
                   
            pluginpath = p.DeleteInvalidPlungin(pluginpath);

            foreach (string filename in pluginpath)
            {
                try
                {
                    //获取文件名
                    string asmfile = filename;
                    string asmname = Path.GetFileNameWithoutExtension(asmfile);
                    if (asmname != string.Empty)
                    {
                        // 利用反射,构造DLL文件的实例
                        Assembly asm = Assembly.LoadFile(asmfile);
                        //利用反射,从程序集(DLL)中,提取类,并把此类实例化
                        Type[] t = asm.GetExportedTypes();
                        foreach (Type type in t)
                        {
                            if (type.GetInterface("IPlugin") != null)
                            {
                                IPlugin plugin = (IPlugin)Activator.CreateInstance(type);

                                plugins.Add(plugin);                                
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }

            #endregion

            //通知插件啟動
            foreach (var plugin in plugins)
            {
                plugin.SetInstance(p);
                plugin.onLoading();
            }

            //啟動事件偵聽任務
            EventTask();

            LSManager.Instance.Test();
            //偵測停止指令 以及核心迴圈
            while (true)
            {                
                string input = Console.ReadLine();

                if (input.Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    break; // 停止程序
                }                                                
            }            

            //通知插件關閉
            foreach (var plugin in plugins)
            {
                plugin.onCloseing();
            }

            //關閉PLC連線線程
            foreach (var device in NetDeviceList)
            {
                device.Value.ConnectClose();
            }
            

        }
        //查找所有插件的路径
        private List<string> FindPlugin()
        {
            List<string> pluginpath = new List<string>();
            try
            {
                //获取程序的基目录
                string path = AppDomain.CurrentDomain.BaseDirectory;
                //合并路径，指向插件所在目录。
                path = Path.Combine(path, "Plugins");
                foreach (string filename in Directory.GetFiles(path, "*.dll"))
                {
                    pluginpath.Add(filename);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return pluginpath;
        }
        //载入插件，在Assembly中查找类型
        private object LoadObject(Assembly asm, string className, string interfacename
                        , object[] param)
        {
            try
            {
                //取得className的类型
                Type t = asm.GetType(className);
                if (t == null
                    || !t.IsClass
                    || !t.IsPublic
                    || t.IsAbstract
                    || t.GetInterface(interfacename) == null
                   )
                {
                    return null;
                }
                //创建对象
                Object o = Activator.CreateInstance(t, param);
                if (o == null)
                {
                    //创建失败，返回null
                    return null;
                }
                return o;
            }
            catch
            {
                return null;
            }
        }
        //移除无效的的插件，返回正确的插件路径列表，Invalid:无效的
        private List<string> DeleteInvalidPlungin(List<string> PlunginPath)
        {
            string interfacename = typeof(IPlugin).FullName;
            List<string> rightPluginPath = new List<string>();
            //遍历所有插件。
            foreach (string filename in PlunginPath)
            {
                try
                {
                    Assembly asm = Assembly.LoadFile(filename);
                    //遍历导出插件的类。
                    foreach (Type t in asm.GetExportedTypes())
                    {
                        //查找指定接口
                        Object plugin = LoadObject(asm, t.FullName, interfacename, null);
                        //如果找到，将插件路径添加到rightPluginPath列表里，并结束循环。
                        if (plugin != null)
                        {
                            rightPluginPath.Add(filename);
                            break;
                        }
                    }
                }
                catch
                {
                    //throw new Exception(filename + "不是有效插件");
                    Console.WriteLine(filename + "不是有效插件");
                }
            }
            return rightPluginPath;
        }

        private void OnProgramCreated()
        {
            ProgramCreated?.Invoke(this, EventArgs.Empty);
        }
        private static void CommunicationTask()
        {

            // 指定文件夹路径
            var dirPath = System.IO.Directory.GetCurrentDirectory();
            string folderPath = $"{dirPath}/DeviceConfig";

            // 获取指定文件夹下所有的 JSON 文件路径
            string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

            // 遍历每个文件路径
            foreach (string filePath in jsonFiles)
            {
                try
                {
                    // 读取 JSON 文件内容
                    string jsonContent = File.ReadAllText(filePath);

                    // 将 JSON 字符串转换为你的类对象
                    BaseDeviceConfigModel obj = JsonConvert.DeserializeObject<BaseDeviceConfigModel>(jsonContent);
                    if (obj.CommunicationInterface == CommunicationInterface.Serial)
                    {
                        Console.WriteLine("找到配置檔[ 串口 ]設備" + filePath);
                    }

                    if (obj.CommunicationInterface == CommunicationInterface.Ethernet)
                    {
                        EthernetDeviceConfigModel ethModel = JsonConvert.DeserializeObject<EthernetDeviceConfigModel>(jsonContent);
                        string deviceName = ethModel.DeviceName;
                        string ip = ethModel.IP;
                        int port = ethModel.Port;

                        var protocolRes = DecodeEthernetDeviceProtocol(ethModel.CommunicationProtocol);

                        if (protocolRes.result)
                        {                     
                            NetworkDeviceBase plc = protocolRes.plcBase;
                            plc.IpAddress = ip;
                            plc.Port = port;
                            plc.ConnectTimeOut = 1000;
                            plc.ReceiveTimeOut = 100;                            
                            plc.ConnectServerAsync();
                            Console.WriteLine("找到配置檔[ 網路 ]設備" + filePath);
                            NetDeviceList.TryAdd(deviceName, plc);
                            Console.WriteLine("添加設備" + deviceName);
                        }
                        else
                        {
                            Console.WriteLine("找不到通訊庫" + ethModel.CommunicationProtocol.ToString());
                        }                        
                    }                                        
                }
                catch (Exception ex)
                {
                    // 处理读取或转换错误
                    Console.WriteLine($"[錯誤] 無法讀取配置檔 {filePath} : {ex.Message}");
                }
            }


        }
        private static void EventTask() 
        {
            //bool flag = false;

            Task.Run(async () => {
                Dictionary<string, bool> flagList = new Dictionary<string, bool>();
                foreach (var device in NetDeviceList)
                {
                    flagList.Add(device.Key, false);
                }

                while (true) {
                    try{
                        Test = "Hello world!";
                        foreach (var device in NetDeviceList)
                        {
                            //Console.WriteLine("現有設備: " + device.Value.IpAddress + NetDeviceList.Count);
                            if (device.Value == null) continue;

                            string ip = device.Value.IpAddress;
                            int port = device.Value.Port;

                            var res = ToolManager.CheckTcpConnect(ip, port);
                            //Console.WriteLine("============ping : " + res);
                            if (res && !flagList[device.Key])
                            {
                                foreach (var plugin in plugins)
                                {
                                    plugin.onDeviceConnect(device.Key);
                                }

                                //Console.WriteLine("上+++++++++++ping : " + res);                                                                                                                                               
                            }
                            if (!res && flagList[device.Key])
                            {
                                foreach (var plugin in plugins)
                                {
                                    plugin.onDeviceDisconnect(device.Key);
                                }
                                //Console.WriteLine("下-----------ping : " + res);
                            }
                            flagList[device.Key] = res;
                        }
                        await Task.Delay(200);
                    }
                    catch (Exception ex) { }                                                            
                }                
            });
        }

        public async Task<QJDataArray> GetData(ReadDataModel model) 
        {
            QJDataArray rData = new QJDataArray();
            rData.IsOk = false;
            //判定設備有在列表內
            if (NetDeviceList.TryGetValue(model.DeviceName, out NetworkDeviceBase pack))
            {
                //解析指令包
                switch (model.DatasType)
                {
                    case DataType.Bool:
                        break;
                    case DataType.Int16:
                        var int16Res = await pack.ReadInt16Async(model.Address, model.ReadLength);
                        if (int16Res.IsSuccess) rData.Data = int16Res.Content.Select(x => (object)x).ToArray();
                        if (!int16Res.IsSuccess) rData.Message = ChineseConverter.Convert(int16Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = int16Res.IsSuccess;
                        rData.DataType = model.DatasType;

                        break;
                    case DataType.UInt16:
                        var uint16Res = await pack.ReadUInt16Async(model.Address, model.ReadLength);
                        if (uint16Res.IsSuccess) rData.Data = uint16Res.Content.Select(x => (object)x).ToArray();
                        if (!uint16Res.IsSuccess) rData.Message = ChineseConverter.Convert(uint16Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = uint16Res.IsSuccess;
                        rData.DataType = model.DatasType;

                        break;
                }
            }
            else
            {
                rData.IsOk = false;
                rData.Message = "找不到指定的設備";
            }
            return rData;

        }

        public static (bool result, NetworkDeviceBase plcBase) DecodeEthernetDeviceProtocol(CommunicationProtocol cProtocol)
        {
            switch (cProtocol)
            {
                case CommunicationProtocol.KvHost:
                    {
                        return (true, new HslCommunication.Profinet.Keyence.KeyenceNanoSerialOverTcp());
                    }
                case CommunicationProtocol.McProtocol_Tcp:
                    {
                        return (true, new HslCommunication.Profinet.Melsec.MelsecMcNet());
                    }
                case CommunicationProtocol.Modbus_Tcp:
                    {
                        return (true, new HslCommunication.ModBus.ModbusTcpNet());
                    }
                case CommunicationProtocol.Vigor_Tcp:
                    {
                        return (true, new HslCommunication.Profinet.Vigor.VigorSerialOverTcp());
                    }
            }
            return (true, null);
        }
    }

    public class ToolManager
    {
        //192.168.0.104
        public static bool CheckTcpConnect(string ip, int port)
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    // 開始非同步連接
                    IAsyncResult asyncResult = tcpClient.BeginConnect(ip, port, null, null);
                    DateTime now = DateTime.Now;
                    do
                    {
                        // 等待 100 毫秒
                        SpinWait.SpinUntil(() => false, 100);
                    }
                    // 當非同步連接完成或經過 100 毫秒時結束循環
                    while (!asyncResult.IsCompleted && DateTime.Now.Subtract(now).Milliseconds < 100);

                    if (asyncResult.IsCompleted)
                    {
                        // 完成非同步連接
                        tcpClient.EndConnect(asyncResult);
                        return true;
                    }

                    // 關閉連接
                    tcpClient.Close();

                    if (!asyncResult.IsCompleted)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            return false;
        }


    }
    public class MemoryShareManager
    {
        public static MemoryShareManager instance = new MemoryShareManager();
        public int Data { get; set; }
    }

}