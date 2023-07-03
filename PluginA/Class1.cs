﻿using ConsolePluginTest;
using PluginFramework;

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
                    var keyenceTask = Task.Run(async () => {
                        var keyenceResult = await Core.GetData(new ReadDataModel()
                        {
                            DeviceName = "Keyence8500_1",
                            Address = "DM10",
                            ReadLength = 100,
                            DatasType = DataType.Int16,
                        });
                        if (keyenceResult.IsOk)
                        {
                            Console.WriteLine(string.Format(PluginName + "Keyence執行狀態:{0} 數據: {1}", keyenceResult.IsOk, DecodeData(keyenceResult)));
                        }
                        else
                        {
                            Console.WriteLine(string.Format(PluginName + "Keyence執行狀態:{0} 錯誤訊息: {1}", keyenceResult.IsOk, keyenceResult.Message));
                        }
                    }) ;

                    var vigorTask = Task.Run(async () => {
                        var vigorResult = await Core.GetData(new ReadDataModel()
                        {
                            DeviceName = "VSM_1",
                            Address = "D10",
                            ReadLength = 100,
                            DatasType = DataType.Int16,
                        });
                        if (vigorResult.IsOk)
                        {
                            Console.WriteLine(string.Format(PluginName + "vigorResult 執行狀態:{0} 數據: {1}", vigorResult.IsOk, DecodeData(vigorResult)));
                        }
                        else
                        {
                            Console.WriteLine(string.Format(PluginName + "vigorResult 執行狀態:{0} 錯誤訊息: {1}", vigorResult.IsOk, vigorResult.Message));
                        }
                    });

                    var keyenceResult = await Core.GetData(new ReadDataModel()
                    {
                        DeviceName = "MC_1",
                        Address = "D500",
                        ReadLength = 1,
                        DatasType = DataType.Int16,
                    });

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