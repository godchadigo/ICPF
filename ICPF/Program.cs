using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using ICPF.Config;
using ICPF.Model;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using Newtonsoft.Json;
using PluginFramework;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ICPFCore
{
    public class PluginBase : IPlugin
    {
        public virtual string Uuid { get; set; } = Guid.NewGuid().ToString();
        public virtual string PluginName { get; set; } = "Base";
        public static Program Core { get; set; }        
        public virtual void onLoading()
        {            
            Console.WriteLine(PluginName + " Loading...");
        }
        public virtual void onCloseing() 
        {
            Console.WriteLine(PluginName + " Closeing...");
        }
        public virtual void CommandTrig(string args) { }  
        public void SetInstance(object dd)
        { 
            Core = (Program) dd;
        }
        public virtual async Task<QJDataArray> GetData(ReadDataModel model) 
        {
            return await Core.GetData(model);
        }
        public virtual async Task<QJDataArray> SetData(WriteDataModel model) 
        {
            return await Core.SetData(model);
        }
        public virtual OperationResult<QJTagData> GetTag(string deviceName, string tagName)
        {
            return Core.GetTag(deviceName, tagName);
        }
        public virtual async Task<OperationResult<QJTagData>> GetTagAsync(string deviceName, string tagName)
        {
            return await Core.GetTagAsync(deviceName, tagName);
        }
        public virtual OperationResult<List<Tag>> GetTagList(string deviceName)
        {
            return Core.GetTagList(deviceName);
        }
        public virtual async Task<OperationResult<List<Tag>>> GetTagListAsync(string deviceName)
        {
            return await Core.GetTagListAsync(deviceName);
        }
        public virtual OperationResult<List<string>> GetMachins()
        {
            return Core.GetMachins();
        }
        public virtual async Task<OperationResult<ConcurrentDictionary<string, QJTagData>>> GetDeviceContainerAsync(string deviceName)
        {
            return await Core.GetDeviceContainerAsync(deviceName);
        }
        public virtual OperationResult<ConcurrentDictionary<string, QJTagData>> GetDeviceContainer(string deviceName)
        {
            return Core.GetDeviceContainer(deviceName);
        }

    }

    public enum IRWDataOperation
    {
        ReadData = 1,
        WriteData = 2,
        Command = 3
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
        public IRWDataCommand iRWCommand { get;set; }

    }
    public class ReadDataModel : IRWData
    {
        public string Uuid { get; set; }
        public string DeviceName { get; set; }
        public string Address { get; set; }
        public ushort ReadLength { get; set; }
        public DataType DatasType { get; set; }
        public IRWDataOperation iRWDataOperation { get; } = IRWDataOperation.ReadData;

    }
    public class WriteDataModel : IRWData
    {
        public string Uuid { get; set; }
        public string DeviceName { get; set; }
        public string Address { get; set; }
        public object[] Datas { get; set; }
        public DataType DatasType { get; set; }
        public IRWDataOperation iRWDataOperation { get; } = IRWDataOperation.WriteData;
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
    public class QJDataArray
    {
        public string Uuid { get; set; }
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public object[] Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
    }
    public class QJDataArray2
    {
        public string Uuid { get; set; }
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public List<QJData> Data { get; set; } = new List<QJData>();        
        public string Message { get; set; }
    }
    public class QJDataList
    {
        public bool IsOk { get; set; }
        public List<object> Data { get; set; }
        public DataType DataType { get; set; }
        public string Message { get; set; }
    }
    public class QJTagData : QJData
    {
        public string GroupName { get; set; }
        public string TagName { get; set; }
    }
    public class QJTagDataArray : QJDataArray
    {        
        public string TagName { get; set; }
    }
    public class QJTagGroupDataArray : QJDataArray
    {        
        public string TagName { get; set; }
        public Dictionary<string , QJDataArray> TagData { get; set; }
    }
    public class OperationResult<T>
    {
        public string Uuid { get; set; }
        public bool IsOk { get; set; }
        public string DeviceName { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
    /// <summary>
    /// v1.初始化，支持單一數據讀取
    /// </summary>
    /// <remarks>
    /// <para>  一個Tag需要具備基礎的資料如下:                     </para>
    /// <para>  1.狀態:用於顯示是否獲取成功                        </para>
    /// <para>  2.     </para> 
    /// <para>  3.目標地址:用於告知ICPF要向哪一個地址發出請求      </para> 
    /// <para>  4.資料類型:用於告知ICPF操作的類型                  </para> 
    /// <para>  5.訊息: 用於告知發生什麼狀態                       </para>
    /// <para>  6.TagName:標籤的名稱，用於給方法查詢               </para>
    /// </remarks>
    public class Tag
    {
        public string GroupName { get; set; }
        public string TagName { get; set; }
        public bool IsOk { get; set; } = false;        
        public string Address { get; set; }
        public DataType DataType { get; set; }
        public ushort Length { get; } = 1;
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
        SiemensS7Net = 5
    }
    public interface IDeviceConfig
    {        
        string DeviceName { get; set; }
        CommunicationInterface CommunicationInterface { get; }
        CommunicationProtocol CommunicationProtocol { get; set; }
        List<Tag> TagList { get; set; } 
    }
    /// <summary>
    /// 基類通訊模型
    /// </summary>
    public class BaseDeviceConfigModel : IDeviceConfig
    {
        public string DeviceName { get; set; }
        public CommunicationInterface CommunicationInterface { get; set; }
        public CommunicationProtocol CommunicationProtocol { get; set; }
        public List<Tag> TagList { get; set; } = new List<Tag>();

    }

    /// <summary>
    /// 網路型通訊模型
    /// </summary>
    public class EthernetDeviceConfigModel : BaseDeviceConfigModel , IDeviceConfig
    {
        public string DeviceName { get; set; }
        public CommunicationInterface CommunicationInterface { get; } = CommunicationInterface.Ethernet;
        public CommunicationProtocol CommunicationProtocol { get; set; }
        public string IP { get;set; }
        public int Port { get;set; }
        public List<Tag> TagList { get; set; } = new List<Tag>();

    }
    /// <summary>
    /// 串口暫不開發
    /// </summary>
    public class SerialDeviceConfigModel : BaseDeviceConfigModel , IDeviceConfig
    {
        public string DeviceName { get; set; }
        public CommunicationInterface CommunicationInterface { get; set; } = CommunicationInterface.Serial;
        public CommunicationProtocol CommunicationProtocol { get; set; }
        public string ComName { get; set; }
        
    }
    public class Program 
    {
        public static event EventHandler<EventArgs> ProgramCreated;        
        private static ConcurrentDictionary<string , DoubleNetworkBase> NetDeviceList = new ConcurrentDictionary<string , DoubleNetworkBase>();
        private static ConcurrentDictionary<string, IDeviceConfig> ConfigList = new ConcurrentDictionary<string, IDeviceConfig>();
        private static ConcurrentBag<QJTagData> Container = new ConcurrentBag<QJTagData>();

        private static Program p;
        private static List<IPlugin> plugins = new List<IPlugin>();
        public static string Test { get; set; } = "test123456";
        private static List<LoadDll>  AsmList = new List<LoadDll>();

        private static Timer timer;
        private static int elapsedTime;
        private const int MaxElapsedTime = 60 * 60 * 24; // 授權鎖秒數
        /// <summary>
        /// 允許使用幾次測試
        /// </summary>
        private const int ForTestCounter = 1;
        /// <summary>
        /// 允許使用幾次測試(當前計數)
        /// </summary>
        private static int ForTestCount = 0;    
        private static bool isAuthorized = false;
        public static Program GetInstance() 
        {
            return p;
        }
        
        static void Main(string[] args)
        {
            isAuthorized = true;
            //***** +測試空間+ *****//
            EthernetDeviceConfigModel ethModel = new EthernetDeviceConfigModel();
            ethModel.DeviceName = "MBUS_2";
            ethModel.CommunicationProtocol = CommunicationProtocol.KvHost;
            ethModel.IP = "127.0.0.1";
            ethModel.Port = 502;

            //新增Tag列表
            ethModel.TagList.Add(new Tag() {
                TagName = "1F溫度表_溫度",
                Address = "0",
                DataType = DataType.UInt16,
            });
            ethModel.TagList.Add(new Tag()
            {
                TagName = "2F溫度表_溫度",
                Address = "1",
                DataType = DataType.UInt16,
            });
            ethModel.TagList.Add(new Tag()
            {
                TagName = "1F溫度表_濕度",
                Address = "2",
                DataType = DataType.UInt16,
            });
            ethModel.TagList.Add(new Tag()
            {
                TagName = "2F溫度表_濕度",
                Address = "3",
                DataType = DataType.UInt16,
            });
            string jsonString = JsonConvert.SerializeObject(ethModel , Formatting.Indented);
            var dirPath = System.IO.Directory.GetCurrentDirectory();
            // 指定本地文件路径
            string filePath = $"{dirPath}/DeviceConfig/Modbus.json";
            //File.WriteAllText(filePath, jsonString);
            //***** -測試空間- *****//
            //return;

            //暫時授權
            //ForTest();

            // 初始化计时器
            timer = new Timer(TimerCallback, null, 0, 1000); // 设置计时器间隔为1秒
            
            //本體
            p = new Program();

            CommunicationTask();
            //啟動刷新器
            GetInstance().ContainerRefresher();
            #region 插件反射載入

            //LoadPlugins();

            List<string> pluginpath = p.FindPlugin();
            //pluginpath = p.DeleteInvalidPlungin(pluginpath);                        
            foreach (string filename in pluginpath)
            {
                AsmList.Add(LoadDLL(filename));
            }

            #endregion

            #region 通知插件啟動            
            foreach (var plugin in AsmList)
            {
                //通知插件啟動任務
                plugin.StartTask();
                //傳入ICPF主體，以便各插件後續可以直接調用主體的方法
                plugin._task.SetInstance(p);                
            }

            #endregion

            #region 啟動設備上下線事件偵聽任務            
            EventTask();
            #endregion

            #region 授權檢測
            if (!isAuthorized)
            {
                Console.WriteLine("----------------------------------");
                Console.WriteLine("----------------------------------");
                Console.WriteLine("----------------------------------");
                Console.WriteLine("---------- ![授權失敗]! ----------");
                Console.WriteLine("----------------------------------");
                Console.WriteLine("----------------------------------");
                Console.WriteLine("----------------------------------");
                Console.WriteLine("按一下離開!"); 
                Console.ReadKey();
                return;
            }
            else
            {
                Console.WriteLine("++++++++++++++++++++++++++++++++++");
                Console.WriteLine("++++++++++++++++++++++++++++++++++");
                Console.WriteLine("++++++++++++++++++++++++++++++++++");
                Console.WriteLine("++++++++++ ![授權成功]! ++++++++++");
                Console.WriteLine("++++++++++++++++++++++++++++++++++");
                Console.WriteLine("++++++++++++++++++++++++++++++++++");
                Console.WriteLine("++++++++++++++++++++++++++++++++++");
            }
            #endregion

            #region 控制台指令偵測器                        
            while (true)
            {                
                string input1 = Console.ReadLine();  //當街收到指令時中斷進入下面的˙判斷

                var iargs = input1.Split(' ');

                string input = iargs[0];

                foreach (var plugin in AsmList)
                {
                    //通知插件指令觸發
                    plugin._task.CommandTrig(input1);                    
                }

                if (input.Equals("stop", StringComparison.OrdinalIgnoreCase))
                {

                    //通知插件關閉
                    foreach (var plugin in AsmList)
                    {
                        plugin.StopTask();                        
                    }

                    //關閉PLC連線線程
                    foreach (var device in NetDeviceList)
                    {
                        device.Value.DeviceBase.ConnectClose();
                    }
                    NetDeviceList = null;
                    Console.WriteLine("隨意按下任何按鍵即可退出...");
                    Console.ReadKey();
                    break; // 停止程序
                }
                if (input.Equals("reload", StringComparison.OrdinalIgnoreCase))
                {
                    ReloadPlugin();   
                    foreach(var device in NetDeviceList)
                    {
                        if (device.Value != null)
                        {
                            device.Value.DeviceBase.ConnectClose();                            
                        }                        
                    }
                    CommunicationTask();
                }
                if (input.Equals("unload", StringComparison.OrdinalIgnoreCase))
                {
                    UnloadPlugin();
                }
                if (input.Equals("load", StringComparison.OrdinalIgnoreCase))
                {
                    LoadPlugin();
                }
                if (input.Equals("plugins", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                    WriteLine(GetPlugins());
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                if (input.Equals("machins", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                    var result = GetInstance().GetMachins();
                    foreach (var device in result.Data)
                    {
                        WriteLine("找到設備" + device);
                    }                    
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                if (input.Equals("getDeviceContainer", StringComparison.OrdinalIgnoreCase))
                {
                    
                    if (iargs.Length == 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.BackgroundColor = ConsoleColor.Black;
                        var result = GetInstance().GetDeviceContainer(iargs[1]);
                        foreach (var item in result.Data)
                        {
                            var data = item.Value;
                            Console.WriteLine($"Uuid:{data.Uuid} DeviceName:{data.DeviceName} Name:{item.Key} isOk:{data.IsOk} Address:{data.Address} Value:{data.Data} DataType:{data.DataType} Message:{data.Message}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.WriteLine("缺少參數!");
                    }
                    
                }
                if (input.Equals("getTag", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                    if (iargs.Length == 3)
                    {
                        var result = GetInstance().GetTag(iargs[1], iargs[2]);
                        var data = result.Data;                        
                        if (result.IsOk)
                            Console.WriteLine($"Uuid:{data.Uuid} DeviceName:{data.DeviceName} isOk:{data.IsOk} Address:{data.Address} Value:{data.Data} DataType:{data.DataType} Message:{data.Message}");
                        else
                            Console.WriteLine(result.Message);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.WriteLine("缺少參數!");
                    }                    
                }
                if (input.Equals("getTagGroup", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                    if (iargs.Length == 3)
                    {
                        var result = GetInstance().GetTagGroup(iargs[1], iargs[2]).WaitAsync(TimeSpan.FromMilliseconds(100));    
                        if (result.Result.IsOk)
                        {
                            foreach (var item in result.Result.Data)
                            {
                                var data = item;
                                if (result.Result.IsOk)
                                    Console.WriteLine($"Uuid:{data.Uuid} DeviceName:{data.DeviceName} isOk:{data.IsOk} Address:{data.Address} Value:{data.Data} Data:{data.Data} DataType:{data.DataType} Message:{data.Message}");
                                else
                                    Console.WriteLine(result.Result.Message);
                            }
                        }                                                    
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.WriteLine("缺少參數!");
                    }
                }
                if (input.Equals("getContainer", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                    foreach (var item in Container)
                    {
                        var data = item;
                        Console.WriteLine($"Uuid:{data.Uuid} TagName:{data.TagName} DeviceName:{data.DeviceName} isOk:{data.IsOk} Address:{data.Address} Data:{item.Data} DataType:{data.DataType} Message:{data.Message}");                        
                    }                    
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            }
            #endregion
            
        }
        public static string GetPlugins()
        {
            string str = string.Empty;
            foreach (var plugin in AsmList)
            {
                str += plugin._task.PluginName + ",";
            }
            return str;
        }
        public static void StartPlugin()
        {
            foreach (var plugin in AsmList)
            {
                plugin.StartTask(); //啟動插件任務
            }
        }
        public static void StopPlugin()
        {
            foreach (var plugin in AsmList)
            {
                plugin.StopTask();
            }
        }
        public static void LoadPlugin()
        {
            if (!isAuthorized) return;
            List<string> pluginpath = p.FindPlugin();
            foreach (string filename in pluginpath)
            {
                var asmPlugin = LoadDLL(filename);
                asmPlugin._task.SetInstance(p);
                AsmList.Add(asmPlugin);
                Console.WriteLine(asmPlugin._task.PluginName + "重啟中...");
            }
            Console.WriteLine("載入了" + AsmList.Count + "份插件。");
            StartPlugin();
        }
        public static void UnloadPlugin()
        {
            Console.WriteLine("卸載插件中...，當前插件數量" + AsmList.Count + "份。");
            StopPlugin();
            AsmList.Clear();
            AsmList = new List<LoadDll>();            
        }
        public static void ReloadPlugin()
        {
            if (!isAuthorized) return;
            UnloadPlugin();            
            LoadPlugin();
            WriteLine("################### Reloading Start... ###################");
            foreach (var asm in AsmList)
            {
                WriteLine(asm._task.PluginName);
            }
            WriteLine("成功載入了" + AsmList.Count + "份插件。");
            WriteLine("#################### Reloading End... ####################");
            
        }
        public static void WriteLine(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        #region Config Manager
        
        /// <summary>
        /// 創建新的配置檔
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="configName"></param>
        /// <returns></returns>
        public (bool isExist , Config config) CreateConfig(PluginBase pb , string configName)
        {
            Config config = new Config();
            foreach (var asm in AsmList)
            {
                if (asm._task.Uuid == pb.Uuid)
                {
                    if (asm.filepath != null)
                    {
                        var newDir = Path.Combine(asm.dirpath, asm._task.PluginName);
                        if (!Directory.Exists(asm.filepath))
                        {                            
                            Directory.CreateDirectory(newDir);
                        }
                       
                        var newDirPath = Path.Combine(newDir, configName + ".json");
                        if (!File.Exists(newDirPath))
                        {
                            using (File.Create(newDirPath))
                            {
                                Console.WriteLine(string.Format("Create Config : PluginName:{0} PluginPath:{1}", asm._task.PluginName, newDirPath));
                                config.configFilePath = newDirPath;
                                config.configDirPath = newDir;
                                config.configName = configName;
                                config.Load();
                                
                                return (false, config);
                            }
                        }
                        else
                        {
                            config.configFilePath = newDirPath;
                            config.configDirPath = newDir;
                            config.configName = configName;
                            config.Load();
                            return (true, config);
                        }                                                
                    }                    
                }
            }
            return (false , config);
        }
        public void RemoveConfig() { }
        

        #endregion
        public static LoadDll LoadDLL(string filePath)
        {
            var load = new LoadDll();
            load.LoadFile(filePath);
            return load;
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
                  
        /// <summary>
        /// 啟動主通訊任務
        /// </summary>
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
                            DoubleNetworkBase doubleBase = new DoubleNetworkBase();
                            NetworkDeviceBase plc = protocolRes.plcBase;
                            plc.IpAddress = ip;
                            plc.Port = port;
                            plc.ConnectTimeOut = 1000;
                            plc.ReceiveTimeOut = 100;
                            plc.ConnectServerAsync();
                            Console.WriteLine("找到配置檔[ 網路 ]設備" + filePath);

                            doubleBase.DeviceBase = plc;

                            doubleBase.TagList = ethModel.TagList;
                                                     
                            doubleBase.Loop = new Task(async () => {
                                while (true)
                                {
                                    //doubleBase.Container.Clear();
                                    foreach (var tag in doubleBase.TagList)
                                    {
                                        ReadDataModel model = new ReadDataModel();
                                        model.Address = tag.Address;
                                        model.ReadLength = tag.Length;
                                        model.DeviceName = ethModel.DeviceName;
                                        model.DatasType = tag.DataType;
                                        model.Uuid = Guid.NewGuid().ToString();



                                        var result = await GetInstance().GetData2(model);
                                        if (result.IsOk)
                                        {
                                            bool isFirst = true;
                                            foreach (var item in result.Data)
                                            {
                                                QJTagData rData = new QJTagData();
                                                rData.Uuid = item.Uuid;
                                                rData.IsOk = item.IsOk;
                                                rData.Address = item.Address;
                                                rData.DataType = item.DataType;
                                                rData.DeviceName = item.DeviceName;
                                                rData.Message = item.Message;
                                                rData.Data = item.Data;

                                                if (isFirst)
                                                {
                                                    rData.GroupName = tag.GroupName;
                                                    rData.TagName = tag.TagName;
                                                    doubleBase.Container.AddOrUpdate(tag.TagName, rData, (key, existingData) => {
                                                        // 如果存在相同的键（数据名称），更新数据                                                        
                                                        existingData = rData;
                                                        return existingData;
                                                    });
                                                    isFirst = false;
                                                }
                                                else
                                                {
                                                    doubleBase.Container.AddOrUpdate(item.Address, rData, (key, existingData) => {
                                                        // 如果存在相同的键（数据名称），更新数据
                                                        existingData = rData;
                                                        return existingData;
                                                    });
                                                }                                                                                                                                         
                                            }
                                        }
                                    }
                                    
                                    foreach (var item in doubleBase.Container)
                                    {
                                        var data = item.Value;
                                        if (!data.IsOk) continue;
                                        //Console.WriteLine($"Uuid:{data.Uuid} DeviceName:{data.DeviceName} Name:{item.Key} isOk:{data.IsOk} Address:{data.Address} Value:{data.Data} DataType:{data.DataType} Message:{data.Message}");

                                        await Task.Delay(10);
                                    }
                                    await Task.Delay(100);
                                }
                            });
                            doubleBase.Loop.Start();
                            NetDeviceList.TryAdd(deviceName, doubleBase);
                            Console.WriteLine("添加設備" + deviceName);
                            ConfigList.TryAdd(deviceName, ethModel);
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
        /// <summary>
        /// 設備容器刷新器
        /// </summary>
        private void ContainerRefresher()
        {
            
            Task.Run(() =>
            {
                while (true)
                {
                    if (NetDeviceList == null) return;
                    foreach (var item in NetDeviceList)
                    {
                        foreach (var data in item.Value.Container)
                        {                                                   
                            // 檢查是否已經存在相同的物件
                            QJTagData existingData = Container.FirstOrDefault(x => x.Address == data.Value.Address );
                            if (existingData != null)
                            {
                                // 如果存在，更新現有物件
                                // 這裡假設你有一個方法來更新物件的內容                                
                                existingData.Data = data.Value.Data;
                                existingData.Uuid = data.Value.Uuid;
                                existingData.Message = data.Value.Message;
                                existingData.DataType = data.Value.DataType;
                                existingData.DeviceName = data.Value.DeviceName;
                                existingData.IsOk = data.Value.IsOk;
                            }
                            else
                            {
                                // 如果不存在，新增物件
                                Container.Add(data.Value);
                            }
                        }
                    }
                }
            });
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
                        if (NetDeviceList == null) continue;
                        foreach (var device in NetDeviceList)
                        {
                            //Console.WriteLine("現有設備: " + device.Value.IpAddress + NetDeviceList.Count);
                            if (device.Value == null) continue;

                            string ip = device.Value.DeviceBase.IpAddress;
                            int port = device.Value.DeviceBase.Port;

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
        public static string[] SplitAlphaNumeric(string input)
        {
            // 使用正则表达式将英文字符和数字分开
            return Regex.Split(input, @"(?<=[a-zA-Z])(?=\d)|(?<=\d)(?=[a-zA-Z])");
        }
        public async Task<QJDataArray2> GetData2(ReadDataModel model)
        {
            return await Task.Run(async () =>
            {
                if (NetDeviceList == null) return new QJDataArray2() { IsOk = false };
                QJDataArray2 rData = new QJDataArray2();
                rData.Uuid = model.Uuid;
                rData.IsOk = false;
                //判定設備有在列表內
                if (NetDeviceList.TryGetValue(model.DeviceName, out DoubleNetworkBase pack))
                {
                    //解析指令包
                    switch (model.DatasType)
                    {
                        case DataType.Bool:
                            var boolRes = await pack.DeviceBase.ReadBoolAsync(model.Address, model.ReadLength);
                            var boolCount = 0;
                            if (boolRes.IsSuccess)
                            {
                                foreach (var item in boolRes.Content)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += boolCount;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += boolCount;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        IsOk = boolRes.IsSuccess,
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = boolRes.Content[boolCount],
                                        Message = "讀取成功!",
                                    });
                                    boolCount++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < model.ReadLength; i++)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += i;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += i;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = 0,
                                        Message = "讀取bool錯誤!"
                                    }); ;
                                }
                            }
                            rData.IsOk = true;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.Int16:
                            var int16Res = await pack.DeviceBase.ReadInt16Async(model.Address, model.ReadLength);
                            var int16Count = 0;
                            if (int16Res.IsSuccess)
                            {
                                foreach (var item in int16Res.Content)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += int16Count;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += int16Count;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        IsOk = int16Res.IsSuccess,
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = int16Res.Content[int16Count],
                                        Message = "讀取成功!",
                                    });
                                    int16Count++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < model.ReadLength; i++)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += i;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += i;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = 0,
                                        Message = "讀取Int16錯誤!"
                                    }); ;
                                }
                            }                            
                            rData.IsOk = true;                            
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.UInt16:
                            var uint16Res = await pack.DeviceBase.ReadUInt16Async(model.Address, model.ReadLength);
                            var uint16Count = 0;
                            if (uint16Res.IsSuccess)
                            {
                                foreach (var item in uint16Res.Content)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += uint16Count;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += uint16Count;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        IsOk = uint16Res.IsSuccess,
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = uint16Res.Content[uint16Count],
                                        Message = "讀取成功!",
                                    });
                                    uint16Count++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < model.ReadLength; i++)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += i;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += i;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = 0,
                                        Message = "讀取uint16錯誤!"
                                    }); ;
                                }
                            }
                            rData.IsOk = true;
                            rData.DeviceName = model.DeviceName;
                            break;

                        case DataType.Int32:
                            var int32Res = await pack.DeviceBase.ReadInt32Async(model.Address, model.ReadLength);
                            var int32Count = 0;
                            if (int32Res.IsSuccess)
                            {
                                foreach (var item in int32Res.Content)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += int32Count;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += int32Count;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        IsOk = int32Res.IsSuccess,
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = int32Res.Content[int32Count],
                                        Message = "讀取成功!",
                                    });
                                    int32Count++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < model.ReadLength; i++)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += i;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += i;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = 0,
                                        Message = "讀取int32錯誤!"
                                    }); ;
                                }
                            }
                            rData.IsOk = true;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.UInt32:
                            var uint32Res = await pack.DeviceBase.ReadUInt32Async(model.Address, model.ReadLength);
                            var uint32Count = 0;
                            if (uint32Res.IsSuccess)
                            {
                                foreach (var item in uint32Res.Content)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += uint32Count;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += uint32Count;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        IsOk = uint32Res.IsSuccess,
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = uint32Res.Content[uint32Count],
                                        Message = "讀取成功!",
                                    });
                                    uint32Count++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < model.ReadLength; i++)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += i;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += i;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = 0,
                                        Message = "讀取uint32錯誤!"
                                    }); ;
                                }
                            }
                            rData.IsOk = true;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.Int64:
                            var int64Res = await pack.DeviceBase.ReadInt64Async(model.Address, model.ReadLength);
                            var int64Count = 0;
                            if (int64Res.IsSuccess)
                            {
                                foreach (var item in int64Res.Content)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += int64Count;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += int64Count;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        IsOk = int64Res.IsSuccess,
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = int64Res.Content[int64Count],
                                        Message = "讀取成功!",
                                    });
                                    int64Count++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < model.ReadLength; i++)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += i;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += i;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = 0,
                                        Message = "讀取int64錯誤!"
                                    }); ;
                                }
                            }
                            rData.IsOk = true;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.UInt64:
                            var uint64Res = await pack.DeviceBase.ReadUInt64Async(model.Address, model.ReadLength);
                            var uint64Count = 0;
                            if (uint64Res.IsSuccess)
                            {
                                foreach (var item in uint64Res.Content)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += uint64Count;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += uint64Count;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        IsOk = uint64Res.IsSuccess,
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = uint64Res.Content[uint64Count],
                                        Message = "讀取成功!",
                                    });
                                    uint64Count++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < model.ReadLength; i++)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += i;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += i;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = 0,
                                        Message = "讀取uint64錯誤!"
                                    }); ;
                                }
                            }
                            rData.IsOk = true;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.Float:
                            var floatRes = await pack.DeviceBase.ReadFloatAsync(model.Address, model.ReadLength);
                            var floatCount = 0;
                            if (floatRes.IsSuccess)
                            {
                                foreach (var item in floatRes.Content)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += floatCount;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += floatCount;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        IsOk = floatRes.IsSuccess,
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = floatRes.Content[floatCount],
                                        Message = "讀取成功!",
                                    });
                                    floatCount++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < model.ReadLength; i++)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += i;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += i;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = 0,
                                        Message = "讀取float錯誤!"
                                    }); ;
                                }
                            }
                            rData.IsOk = true;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.Double:
                            var doubleRes = await pack.DeviceBase.ReadDoubleAsync(model.Address, model.ReadLength);
                            var doubleCount = 0;
                            if (doubleRes.IsSuccess)
                            {
                                foreach (var item in doubleRes.Content)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += doubleCount;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += doubleCount;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        IsOk = doubleRes.IsSuccess,
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = doubleRes.Content[doubleCount],
                                        Message = "讀取成功!",
                                    });
                                    doubleCount++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < model.ReadLength; i++)
                                {
                                    string realAddress = string.Empty;
                                    var addrNumber = SplitAlphaNumeric(model.Address);
                                    if (addrNumber.Length == 1)
                                    {
                                        if (int.TryParse(addrNumber[0], out int number))
                                        {
                                            number += i;
                                            realAddress = number.ToString();
                                        }
                                    }
                                    else if (addrNumber.Length == 2)
                                    {
                                        if (int.TryParse(addrNumber[1], out int number))
                                        {
                                            number += i;
                                            realAddress = addrNumber[0] + number.ToString();
                                        }
                                    }
                                    rData.Data.Add(new QJData()
                                    {
                                        Uuid = Guid.NewGuid().ToString(),
                                        Address = realAddress,
                                        DeviceName = model.DeviceName,
                                        DataType = model.DatasType,
                                        Data = 0,
                                        Message = "讀取double錯誤!"
                                    }); ;
                                }
                            }
                            rData.IsOk = true;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.String:
                            var stringRes = await pack.DeviceBase.ReadStringAsync(model.Address, model.ReadLength);
                            if (stringRes.IsSuccess)
                            {
                                //rData.Data = stringRes.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(stringRes.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = stringRes.IsSuccess;
                            
                            rData.DeviceName = model.DeviceName;
                            break;
                        default:
                            // 適當的錯誤處理
                            break;
                    }

                }
                else
                {
                    rData.IsOk = false;
                    rData.Message = "找不到指定的設備";
                }
                return rData;
            });
        }


        /// <summary>
        /// 提供讀取模組，執行讀取指令
        /// </summary>
        /// <param name="model"></param>
        /// <returns>通訊讀取完成結果以及陣列數據和狀態</returns>
        public async Task<QJDataArray> GetData(ReadDataModel model) 
        {
            return await Task.Run(async() =>
            {
                QJDataArray rData = new QJDataArray();
                rData.Uuid = model.Uuid;
                rData.IsOk = false;
                //判定設備有在列表內
                if (NetDeviceList.TryGetValue(model.DeviceName, out DoubleNetworkBase pack))
                {                    
                    //解析指令包
                    switch (model.DatasType)
                    {
                        case DataType.Bool:
                            var bool16Res = await pack.DeviceBase.ReadInt16Async(model.Address, model.ReadLength);
                            if (bool16Res.IsSuccess)
                            {
                                rData.Data = bool16Res.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(bool16Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = bool16Res.IsSuccess;
                            rData.DataType = model.DatasType;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.Int16:
                            var int16Res = await pack.DeviceBase.ReadInt16Async(model.Address, model.ReadLength);
                            if (int16Res.IsSuccess)
                            {
                                rData.Data = int16Res.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(int16Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = int16Res.IsSuccess;
                            rData.DataType = model.DatasType;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.UInt16:
                            var uint16Res = await pack.DeviceBase.ReadUInt16Async(model.Address, model.ReadLength);
                            if (uint16Res.IsSuccess)
                            {
                                rData.Data = uint16Res.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(uint16Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = uint16Res.IsSuccess;
                            rData.DataType = model.DatasType;
                            rData.DeviceName = model.DeviceName;
                            break;

                        case DataType.Int32:
                            var int32Res = await pack.DeviceBase.ReadInt32Async(model.Address, model.ReadLength);
                            if (int32Res.IsSuccess)
                            {
                                rData.Data = int32Res.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(int32Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = int32Res.IsSuccess;
                            rData.DataType = model.DatasType;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.UInt32:
                            var uint32Res = await pack.DeviceBase.ReadUInt32Async(model.Address, model.ReadLength);
                            if (uint32Res.IsSuccess)
                            {
                                rData.Data = uint32Res.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(uint32Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = uint32Res.IsSuccess;
                            rData.DataType = model.DatasType;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.Int64:
                            var int64Res = await pack.DeviceBase.ReadInt64Async(model.Address, model.ReadLength);
                            if (int64Res.IsSuccess)
                            {
                                rData.Data = int64Res.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(int64Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = int64Res.IsSuccess;
                            rData.DataType = model.DatasType;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.UInt64:
                            var uint64Res = await pack.DeviceBase.ReadUInt64Async(model.Address, model.ReadLength);
                            if (uint64Res.IsSuccess)
                            {
                                rData.Data = uint64Res.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(uint64Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = uint64Res.IsSuccess;
                            rData.DataType = model.DatasType;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.Float:
                            var floatRes = await pack.DeviceBase.ReadFloatAsync(model.Address, model.ReadLength);
                            if (floatRes.IsSuccess)
                            {
                                rData.Data = floatRes.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(floatRes.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = floatRes.IsSuccess;
                            rData.DataType = model.DatasType;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.Double:
                            var doubleRes = await pack.DeviceBase.ReadDoubleAsync(model.Address, model.ReadLength);
                            if (doubleRes.IsSuccess)
                            {
                                rData.Data = doubleRes.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(doubleRes.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = doubleRes.IsSuccess;
                            rData.DataType = model.DatasType;
                            rData.DeviceName = model.DeviceName;
                            break;
                        case DataType.String:
                            var stringRes = await pack.DeviceBase.ReadStringAsync(model.Address, model.ReadLength);
                            if (stringRes.IsSuccess)
                            {
                                rData.Data = stringRes.Content.Select(x => (object)x).ToArray();
                            }
                            rData.Message = ChineseConverter.Convert(stringRes.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = stringRes.IsSuccess;
                            rData.DataType = model.DatasType;
                            rData.DeviceName = model.DeviceName;
                            break;
                        default:
                            // 適當的錯誤處理
                            break;
                    }

                }
                else
                {
                    rData.IsOk = false;
                    rData.Message = "找不到指定的設備";
                }
                return rData;
            });            
        }
        /// <summary>
        /// 提供寫入模組，執行寫入指令
        /// </summary>
        /// <param name="model"></param>
        /// <returns>通訊寫入完成結果和狀態</returns>
        public async Task<QJDataArray> SetData(WriteDataModel model)
        {
            QJDataArray rData = new QJDataArray();
            rData.Uuid = model.Uuid;
            rData.IsOk = false;
            //判定設備有在列表內
            if (NetDeviceList.TryGetValue(model.DeviceName, out DoubleNetworkBase pack))
            {
                //解析指令包
                switch (model.DatasType)
                {
                    case DataType.Bool:
                        bool[] boolArray = model.Datas.Select(Convert.ToBoolean).ToArray();
                        var bool16Res = await pack.DeviceBase.WriteAsync(model.Address, boolArray);
                        rData.Message = ChineseConverter.Convert(bool16Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = bool16Res.IsSuccess;
                        rData.DeviceName = model.DeviceName;
                        break;
                    case DataType.Int16:
                        short[] shortArray = model.Datas.Select(Convert.ToInt16).ToArray();
                        var int16Res = await pack.DeviceBase.WriteAsync(model.Address, shortArray);
                        rData.Message = ChineseConverter.Convert(int16Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = int16Res.IsSuccess;
                        rData.DeviceName = model.DeviceName;
                        break;
                    case DataType.UInt16:
                        ushort[] ushortArray = model.Datas.Select(Convert.ToUInt16).ToArray();
                        var uint16Res = await pack.DeviceBase.WriteAsync(model.Address, ushortArray);
                        rData.Message = ChineseConverter.Convert(uint16Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = uint16Res.IsSuccess;
                        rData.DeviceName = model.DeviceName;
                        break;
                    case DataType.Int32:
                        int[] intArray = model.Datas.Select(Convert.ToInt32).ToArray();
                        var int32Res = await pack.DeviceBase.WriteAsync(model.Address, intArray);
                        rData.Message = ChineseConverter.Convert(int32Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = int32Res.IsSuccess;
                        rData.DeviceName = model.DeviceName;
                        break;
                    case DataType.UInt32:
                        uint[] uintArray = model.Datas.Select(Convert.ToUInt32).ToArray();
                        var uint32Res = await pack.DeviceBase.WriteAsync(model.Address, uintArray);
                        rData.Message = ChineseConverter.Convert(uint32Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = uint32Res.IsSuccess;
                        rData.DeviceName = model.DeviceName;
                        break;
                    case DataType.Int64:
                        long[] longArray = model.Datas.Select(Convert.ToInt64).ToArray();
                        var int64Res = await pack.DeviceBase.WriteAsync(model.Address, longArray);
                        rData.Message = ChineseConverter.Convert(int64Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = int64Res.IsSuccess;
                        rData.DeviceName = model.DeviceName;
                        break;
                    case DataType.UInt64:
                        ulong[] ulongArray = model.Datas.Select(Convert.ToUInt64).ToArray();
                        var uint64Res = await pack.DeviceBase.WriteAsync(model.Address, ulongArray);
                        rData.Message = ChineseConverter.Convert(uint64Res.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = uint64Res.IsSuccess;
                        rData.DeviceName = model.DeviceName;
                        break;
                    case DataType.Float:
                        float[] floatArray = model.Datas.Select(Convert.ToSingle).ToArray();
                        var floatRes = await pack.DeviceBase.WriteAsync(model.Address, floatArray);
                        rData.Message = ChineseConverter.Convert(floatRes.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = floatRes.IsSuccess;
                        rData.DeviceName = model.DeviceName;
                        break;
                    case DataType.Double:
                        double[] doubleArray = model.Datas.Select(Convert.ToDouble).ToArray();
                        var doubleRes = await pack.DeviceBase.WriteAsync(model.Address, doubleArray);
                        rData.Message = ChineseConverter.Convert(doubleRes.Message, ChineseConversionDirection.SimplifiedToTraditional);
                        rData.IsOk = doubleRes.IsSuccess;
                        rData.DeviceName = model.DeviceName;
                        break;
                    case DataType.String:
                        string[] stringArray = model.Datas.Select(x => x.ToString()).ToArray();
                        if (stringArray.Length == 1)
                        {
                            var stringRes = await pack.DeviceBase.WriteAsync(model.Address, stringArray[0]);
                            rData.Message = ChineseConverter.Convert(stringRes.Message, ChineseConversionDirection.SimplifiedToTraditional);
                            rData.IsOk = stringRes.IsSuccess;
                            rData.DeviceName = model.DeviceName;
                        }                        
                        break;
                    default:
                        // 適當的錯誤處理
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
        public OperationResult<List<Tag>> GetTagList(string deviceName)
        {
            OperationResult<List<Tag>> rData = new OperationResult<List<Tag>>();
            rData.IsOk = false;
            rData.Uuid = Guid.NewGuid().ToString();
            if (ConfigList.TryGetValue(deviceName, out IDeviceConfig model))
            {
                rData.IsOk = true;
                rData.Data = model.TagList;
                return rData;
            }
            else
            {
                rData.IsOk = false;
                rData.Message = "找不到指定的設備!";
                return rData;
            }
            rData.Message = "錯誤，請勿隨意注入惡意程式!!";
            return rData;
        }
        public async Task<OperationResult<List<Tag>>> GetTagListAsync(string deviceName)
        {
            OperationResult<List<Tag>> rData = new OperationResult<List<Tag>>();
            rData.IsOk = false;
            rData.Uuid = Guid.NewGuid().ToString();
            if (ConfigList.TryGetValue(deviceName, out IDeviceConfig model))
            {
                rData.IsOk = true;
                rData.Data = model.TagList;
                return rData;
            }
            else
            {
                rData.IsOk = false;
                rData.Message = "找不到指定的設備!";
                return rData;
            }
            rData.Message = "錯誤，請勿隨意注入惡意程式!!";
            return rData;
        }
        public async Task<OperationResult<List<QJTagData>>> GetTagGroup(string deviceName, string groupName)
        {
            if (groupName.Length == 0) return new OperationResult<List<QJTagData>>() { IsOk = false, Message = "錯誤，群組名稱為空!" };
            if (deviceName.Length == 0) return new OperationResult<List<QJTagData>>() { IsOk = false, Message = "錯誤，設備名稱為空!" };
            if (NetDeviceList.TryGetValue(deviceName, out DoubleNetworkBase deviceModel))
            {
                List<QJTagData> rData = new List<QJTagData>();
                var result = deviceModel.Container.Where(x => x.Value.GroupName == groupName).ToList();
                foreach (var item in result)
                {
                    rData.Add(item.Value);
                }
                if (rData.Count != 0)
                {
                    return new OperationResult<List<QJTagData>>() { IsOk = true, Data = rData, DeviceName = deviceName, Message = "獲取標籤成功!" };
                }
                else
                {
                    return new OperationResult<List<QJTagData>>() { IsOk = false, Message = "錯誤，找不到指定的標籤!" };
                }
            }
            else
            {
                return new OperationResult<List<QJTagData>>() { IsOk = false, Message = "錯誤，找不到指定的設備容器!" };
            }            
        }
        public async Task<OperationResult<QJTagData>> GetTagAsync(string deviceName , string tagName)
        {
            if (tagName.Length == 0) return new OperationResult<QJTagData>() { IsOk = false, Message = "錯誤，標籤名稱為空!" };
            if (deviceName.Length == 0) return new OperationResult<QJTagData>() { IsOk = false, Message = "錯誤，設備名稱為空!" };
            if (NetDeviceList.TryGetValue(deviceName, out DoubleNetworkBase deviceModel))
            {
                if (deviceModel.Container.TryGetValue(tagName , out QJTagData dataModel))
                {
                    return new OperationResult<QJTagData>() { IsOk = true, Data = dataModel, DeviceName = deviceName, Message = "獲取標籤成功!" };
                }
                else
                {
                    return new OperationResult<QJTagData>() { IsOk = false, Message = "錯誤，找不到指定的標籤!" };
                }                                
            }
            else
            {
                return new OperationResult<QJTagData>() { IsOk = false, Message = "錯誤，找不到指定的設備容器!" };
            }            
        }
        public OperationResult<QJTagData> GetTag(string deviceName, string tagName)
        {
            if (tagName.Length == 0) return new OperationResult<QJTagData>() { IsOk = false, Message = "錯誤，標籤名稱為空!" };
            if (deviceName.Length == 0) return new OperationResult<QJTagData>() { IsOk = false, Message = "錯誤，設備名稱為空!" };
            if (NetDeviceList.TryGetValue(deviceName, out DoubleNetworkBase deviceModel))
            {
                if (deviceModel.Container.TryGetValue(tagName, out QJTagData dataModel))
                {
                    return new OperationResult<QJTagData>() { IsOk = true, Data = dataModel, DeviceName = deviceName, Message = "獲取標籤成功!" };
                }
                else
                {
                    return new OperationResult<QJTagData>() { IsOk = false, Message = "錯誤，找不到指定的標籤!" };
                }
            }
            else
            {
                return new OperationResult<QJTagData>() { IsOk = false, Message = "錯誤，找不到指定的設備容器!" };
            }
        }
        public async Task<OperationResult<ConcurrentDictionary<string, QJTagData>>> GetDeviceContainerAsync(string deviceName)
        {
            if(NetDeviceList.TryGetValue(deviceName , out DoubleNetworkBase deviceModel))
            {
                return new OperationResult<ConcurrentDictionary<string, QJTagData>>() { IsOk = true, Data = deviceModel.Container, DeviceName = deviceName, Message = "獲取設備容器成功!" };
            }
            else
            {
                return new OperationResult<ConcurrentDictionary<string, QJTagData>>() { IsOk = false, Message = "錯誤，找不到指定的設備容器!" };
            }
            return new OperationResult<ConcurrentDictionary<string, QJTagData>>() { IsOk = false, Message = "錯誤!" };
        }
        public OperationResult<ConcurrentDictionary<string, QJTagData>> GetDeviceContainer(string deviceName)
        {
            if (NetDeviceList.TryGetValue(deviceName, out DoubleNetworkBase deviceModel))
            {
                return new OperationResult<ConcurrentDictionary<string, QJTagData>>() { IsOk = true, Data = deviceModel.Container, DeviceName = deviceName, Message = "獲取設備容器成功!" };
            }
            else
            {
                return new OperationResult<ConcurrentDictionary<string, QJTagData>>() { IsOk = false, Message = "錯誤，找不到指定的設備容器!" };
            }
            return new OperationResult<ConcurrentDictionary<string, QJTagData>>() { IsOk = false, Message = "錯誤!" };
        }
        /// <summary>
        /// 獲取當前配置黨載入的設備
        /// </summary>
        /// <returns></returns>
        public  OperationResult<List<string>> GetMachins()
        {
            OperationResult<List<string>> rData = new OperationResult<List<string>>();
            rData.Uuid = Guid.NewGuid().ToString();
            rData.Data = new List<string>();
            rData.IsOk = true;                        
            foreach (var device in NetDeviceList)
            {
                rData.Data.Add(device.Key);
            }
            return rData;
        }
        /// <summary>
        /// 解析通訊型通訊，物件實例化
        /// </summary>
        /// <param name="cProtocol"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 授權計時器
        /// </summary>
        /// <param name="state"></param>
        private static void TimerCallback(object state)
        {
            if (elapsedTime >= MaxElapsedTime )
            {
                isAuthorized = false;
                ForTestCount++;
                timer.Dispose();
                Debug.WriteLine("24小時測試授權已達，請重新授權。");
                UnloadPlugin();
            }
            elapsedTime++;
            //Note:一開始先以小時提示，當最後剩下分鐘時10分鐘提示一次，最後一分鐘時數秒。
            if (((MaxElapsedTime-elapsedTime) % (60*60)) == 0 && ((MaxElapsedTime-elapsedTime) / (60 * 60)) != 0 )
            {
                //時
                Debug.WriteLine("剩餘" + (MaxElapsedTime - elapsedTime) % (60*60) + "小時");
            }
            else
            {
                if (((MaxElapsedTime-elapsedTime) % (10 * 60)) == 0 && ((MaxElapsedTime - elapsedTime) / (10 * 60)) != 0)
                {
                    //10分
                    Debug.WriteLine("剩餘" + (MaxElapsedTime - elapsedTime) % (10 * 60) * 10 + "分鐘");
                }
                else
                {
                    if (((MaxElapsedTime - elapsedTime) / (60)) == 0)
                    {
                        //秒
                        Debug.WriteLine("剩餘" + (MaxElapsedTime - elapsedTime) % (60) + "秒鐘");
                    }
                }
            }
            
        }
        /// <summary>
        /// 測試人員暫時測試授權
        /// </summary>
        public void ForTest()
        {
            if (!isAuthorized && ForTestCount < ForTestCounter)
                isAuthorized = true;
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
                    while (!asyncResult.IsCompleted && DateTime.Now.Subtract(now).Milliseconds < 300);

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