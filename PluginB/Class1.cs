using ConsolePluginTest;
using Nancy;
using Nancy.Hosting.Self;
using Newtonsoft;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PluginB
{
    public class PluginB : MemoryShareManager, IPlugin
    {

        class SampleModule : NancyModule
        {
            public SampleModule() {
                Get("/", args =>
                {
                    return Guid.NewGuid().ToString();
                });
            }
        }

        public void onLoading()
        {
            Thread t = new Thread(async () => {
                while (true)
                {
                    var r = new Random();
                    int data = r.Next(0, 1000);
                    Console.WriteLine("PluginB : " + instance.Data);
                    instance.Data = data;
                    //Console.WriteLine(r);
                    await Task.Delay(500);
                }
            });
            t.Start();

        }
        public void onCloseing()
        {
            Console.WriteLine("PluginB Closeing...");            
            
        }

       
    }



}