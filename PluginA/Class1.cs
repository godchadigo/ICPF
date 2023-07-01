using ConsolePluginTest;

namespace PluginA
{

    public class PluginA : IPlugin
    {
        private Program Core;
        private string PluginName = "PluginA";
        private CancellationTokenSource cts = new CancellationTokenSource();


        public void onLoading()
        {
            Console.WriteLine(PluginName + " Loading...");
            // 获取 CancellationToken
            CancellationToken token = cts.Token;
            Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    //Console.WriteLine("PluginA : " + MemoryShareManager.instance.Data);
                    //Core.DoSomething ("PluginA");
                    var result = Core.GetData(new ReadDataModel()
                    {
                        DeviceName = "Modbus_Tcp_工廠_鑄造區_機臺1",
                        Address = "10",
                        ReadLength = 10,
                        DatasType = DataType.Int16,
                    });

                    if (result.IsOk)
                    {
                        Console.WriteLine(string.Format(PluginName + "執行狀態:{0} 數據: {1}", result.IsOk, DecodeData(result)));
                    }
                    else
                    {
                        Console.WriteLine(string.Format(PluginName + "執行狀態:{0} 錯誤訊息: {1}", result.IsOk , result.Message ));
                    }
                    
                    await Task.Delay(1000);
                }
            }, token);

        }

        public void onCloseing()
        {
            cts.Cancel();
            Console.WriteLine(PluginName + " Closeing...");
        }

        public void SetInstance(object dd)
        {
            Program program = (Program)dd;
            Core = program;
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