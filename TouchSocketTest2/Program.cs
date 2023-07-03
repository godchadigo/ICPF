

using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;
using static System.Net.Mime.MediaTypeNames;

namespace TouchSocketTest2
{
    public class Program
    {
        private static TcpClient tcpClient = new TcpClient();
        private static bool flag = false;
        static void Main(string[] args)
        {                        
            tcpClient.Connected += (client, e) => {
                Console.WriteLine("上線");
                tcpClient.Send("RRQM");
                flag = true;
            };//成功连接到服务器
            tcpClient.Disconnected += (client, e) => {
                Console.WriteLine("斷線");
                flag = false;
            };//从服务器断开连接，当连接不成功时不会触发。
            tcpClient.Received += (client, byteBlock, requestInfo) =>
            {
                //从服务器收到信息
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                Console.WriteLine($"接收到信息：{mes}");
            };

            //声明配置
            TouchSocketConfig config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:5000"))
                .UsePlugin()
                .ConfigurePlugins(a =>
                {
                    a.UseReconnection(-1, true, 100);//如需永远尝试连接，tryCount设置为-1即可。
                }); 

            //载入配置
            tcpClient.Setup(config);
            tcpClient.Connect();

            Test();

            Console.ReadKey();
        }
        private static void Test()
        {
            
            Task.Run(async() => {
                while (true)
                {
                    if (flag)
                        tcpClient.Send("SetData DM100 5 10");
                    await Task.Delay(1);
                }
            });
        }

    }
}