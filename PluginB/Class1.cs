using ConsolePluginTest;
using Nancy;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using PluginFramework;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PluginB
{
    public class PluginB : IPlugin
    {
        private Program Core;
        public string PluginName { get; } = "PluginB";
        private CancellationTokenSource cts = new CancellationTokenSource();


        public void onLoading()
        {
            Console.WriteLine(PluginName + " Loading...");
            // 获取 CancellationToken
            CancellationToken token = cts.Token;
            Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    
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