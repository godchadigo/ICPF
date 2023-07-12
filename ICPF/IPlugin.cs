using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginFramework
{
    public interface IPlugin
    {
        string PluginName { get; }
        void onLoading() { }
        void onCloseing() { }
        void onDeviceConnect(string deviceName) { }
        void onDeviceDisconnect(string deviceName) { }
        void SetInstance(object dd) { }        
        /// <summary>
        /// 讀取數據(讀取失敗等待)
        /// </summary>
        void GetData() { }
        /// <summary>
        /// 讀取數據(讀取失敗直接放棄)
        /// </summary>
        void GetDataEx() { }
        /// <summary>
        /// 寫入數據(寫入失敗等待)
        /// </summary>
        void SetData() { }
        /// <summary>
        /// 寫入數據(寫入失敗直接放棄)
        /// </summary>
        void SetDataEx() { }
        /// <summary>
        /// 讀取定義好的標籤
        /// </summary>
        void GetTag() { }
        /// <summary>
        /// 設定定義好的標籤
        /// </summary>
        void SetTag() { }
        /// <summary>
        /// 主迴圈偵聽指令廣播
        /// </summary>
        void CommandTrig(string args) { }
    }
}
