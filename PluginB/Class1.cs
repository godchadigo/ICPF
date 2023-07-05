using ICPFCore;
using Nancy;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using Plugin.B;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace PluginB
{
    public class PluginB : PluginBase 
    {
        public override string PluginName { get; set; } = "PluginB";
        private CancellationTokenSource cts = new CancellationTokenSource();        


        public override void onCloseing()
        {            
            cts.Cancel();
            base.onCloseing();
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