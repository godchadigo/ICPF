using ICPFCore;

namespace PluginA
{

    public class PluginA : PluginBase
    {

        public override string PluginName { get; set; } = "PluginA";
        private CancellationTokenSource cts = new CancellationTokenSource();
        public override void onLoading()
        {
            Core.ForTest();
            Console.WriteLine(PluginName + " Loading...");
            
            CancellationToken token = cts.Token;
            Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    var getTagRes = await GetTag("MBUS_2" , "1F溫度表_溫度");
                    if (getTagRes.IsOk)
                    {
                        Console.WriteLine(string.Format(PluginName + "getTagRes執行狀態:{0} 標籤名稱: {1} 數據: {2}", getTagRes.IsOk, getTagRes.TagName , DecodeData(getTagRes)));
                    }
                    else
                    {
                        Console.WriteLine(string.Format(PluginName + "getTagRes執行狀態:{0} 錯誤訊息: {1}", getTagRes.IsOk, getTagRes.Message));
                    }
                    //Console.WriteLine(DateTime.Now.ToString());
                    await Task.Delay(1000);
                }
            } , cts.Token );

        }
        public override void onCloseing()
        {
            cts.Cancel();
            Console.WriteLine(PluginName + " Closeing...");
        }
        public override void CommandTrig(string args)
        {
            base.CommandTrig(args);
        }

        //***** 事件偵測 *****//
        public void onDeviceConnect(string deviceName)
        {
            Console.WriteLine($"偵測到來自主機端發出的 {deviceName} 設備上線!");
        }
        public void onDeviceDisconnect(string deviceName)
        {
            Console.WriteLine($"偵測到來自主機端發出的 {deviceName} 設備斷線!");
        }
        private string DecodeData(QJDataArray data)
        {
            string temp = string.Empty; 
            if (data.IsOk)
            {
                foreach (var str in data.Data)
                {
                    temp = temp + str + " ";  
                }
                return temp;
            }
            else
            {
                return "";
            }
        }
    }
}