using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPF.Config
{
    public class Config
    {
        public string configFilePath { get; set; } = string.Empty;
        public string configDirPath { get; set; } = string.Empty;
        public string configName { get; set; } = string.Empty;
        public string configBuffer { get;set; } = string.Empty;
        public Dictionary<object , object> configModelBuffer { get; set; }
        public string configSaveBuffer { get;set; } = string.Empty;
        public Config() 
        {
            try
            {
                configBuffer = File.ReadAllText(configFilePath);
                configModelBuffer = Newtonsoft.Json.JsonConvert.DeserializeObject < Dictionary<object, object> > (configBuffer);
            }
            catch (Exception ex)
            {

            }            
        }
        /// <summary>
        /// 獲取配置檔名稱
        /// </summary>
        /// <returns></returns>
        public string GetConfigName()
        {
            return configName;
        }
        /// <summary>
        /// 獲取配置檔完整路徑
        /// </summary>
        /// <returns></returns>
        public string GetConfigFilePath()
        {
            return configFilePath;
        }
        /// <summary>
        /// 獲取配置檔所在的路徑
        /// </summary>
        /// <returns></returns>
        public string GetConfigDirPath()
        {
            return configDirPath;
        }
        //配置檔管理器，需要有創見檔案及資料夾的功能，和讀寫的功能，打算用json製作

        #region 讀取方法
        /// <summary>
        /// 讀取Object類型物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (bool isOk,object value) Read(object key)
        {
            var findRes = configModelBuffer.Where(x => x.Key == key).FirstOrDefault();
            if (findRes.Key != null && findRes.Value != null)
            {
                return (true , findRes.Value);
            }
            return (false , null);
        }
        /// <summary>
        /// 讀取Bool物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (bool isOk, bool value) ReadBool(object key)
        {
            var findRes = configModelBuffer.Where(x => x.Key == key).FirstOrDefault();
            bool result = false;
            if (findRes.Key != null && findRes.Value != null)
            {                
                var convertRes = bool.TryParse(findRes.Value.ToString(), out result);
                return (convertRes, result);
            }
            return (false , result);
        }
        /// <summary>
        /// 讀取Short物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (bool isOk, short value) ReadShort(object key)
        {
            var findRes = configModelBuffer.Where(x => x.Key == key).FirstOrDefault();
            short result = 0;
            if (findRes.Key != null && findRes.Value != null)
            {
                var convertRes = short.TryParse(findRes.Value.ToString(), out result);
                return (convertRes, result);
            }
            return (false, result);
        }
        /// <summary>
        /// 讀取Int物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (bool isOk, int value) ReadInt(object key)
        {
            var findRes = configModelBuffer.FirstOrDefault(x => x.Key == key);
            int result = 0;
            if (findRes.Key != null && findRes.Value != null)
            {
                var convertRes = int.TryParse(findRes.Value.ToString(), out result);
                return (convertRes, result);
            }
            return (false, result);
        }
        /// <summary>
        /// 讀取Long物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (bool isOk, long value) ReadLong(object key)
        {
            var findRes = configModelBuffer.FirstOrDefault(x => x.Key == key);
            long result = 0;
            if (findRes.Key != null && findRes.Value != null)
            {
                var convertRes = long.TryParse(findRes.Value.ToString(), out result);
                return (convertRes, result);
            }
            return (false, result);
        }
        /// <summary>
        /// 讀取Float物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (bool isOk, float value) ReadFloat(object key)
        {
            var findRes = configModelBuffer.FirstOrDefault(x => x.Key == key);
            float result = 0;
            if (findRes.Key != null && findRes.Value != null)
            {
                var convertRes = float.TryParse(findRes.Value.ToString(), out result);
                return (convertRes, result);
            }
            return (false, result);
        }
        /// <summary>
        /// 讀取Double物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (bool isOk, double value) ReadDouble(object key)
        {
            var findRes = configModelBuffer.FirstOrDefault(x => x.Key == key);
            double result = 0;
            if (findRes.Key != null && findRes.Value != null)
            {
                var convertRes = double.TryParse(findRes.Value.ToString(), out result);
                return (convertRes, result);
            }
            return (false, result);
        }
        /// <summary>
        /// 讀取Decimal物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (bool isOk, decimal value) ReadDecimal(object key)
        {
            var findRes = configModelBuffer.FirstOrDefault(x => x.Key == key);
            decimal result = 0;
            if (findRes.Key != null && findRes.Value != null)
            {
                var convertRes = decimal.TryParse(findRes.Value.ToString(), out result);
                return (convertRes, result);
            }
            return (false, result);
        }
        /// <summary>
        /// 讀取String物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (bool isOk, string value) ReadString(object key)
        {
            var findRes = configModelBuffer.FirstOrDefault(x => x.Key == key);
            string result = string.Empty;
            if (findRes.Key != null && findRes.Value != null)
            {
                result = findRes.Value.ToString();
                return (true, result);
            }
            return (false, result);
        }
        #endregion
        
        public void Write(object key , object value)
        {      
            configModelBuffer.Add(key , value);
            configSaveBuffer = Newtonsoft.Json.JsonConvert.SerializeObject(configModelBuffer , Newtonsoft.Json.Formatting.Indented);            
        }
        public bool Remove(object key)
        {
            var findRes = configModelBuffer.FirstOrDefault(x => x.Key == key);            
            if (findRes.Key != null && findRes.Value != null)
            {
                configModelBuffer.Remove(findRes.Key);
                configSaveBuffer = Newtonsoft.Json.JsonConvert.SerializeObject(configModelBuffer, Newtonsoft.Json.Formatting.Indented);
                return true;
            }
            return false;
        }
        public void Init()
        {            
            configModelBuffer = new Dictionary<object, object>();
            configSaveBuffer = Newtonsoft.Json.JsonConvert.SerializeObject(configModelBuffer, Newtonsoft.Json.Formatting.Indented);            
        }
        public void Save()
        {
            try
            {
                File.WriteAllText(configFilePath, configSaveBuffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Save Crash :" + ex.ToString());
            }            
        }
    }
    public class ConfigModel
    {
        public Dictionary<object , object> KVList { get; set; }
    }

}
