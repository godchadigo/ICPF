using TouchSocket.Rpc.WebApi;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.Http;
using TouchSocket.Core;
using System.Text;

public class Program
{
    public static void Main()
    {
        TcpService service = new TcpService();
        service.Connecting = (client, e) => { };//有客户端正在连接
        service.Connected = (client, e) => { };//有客户端成功连接
        service.Disconnected = (client, e) => { };//有客户端断开连接
        service.Received = (client, byteBlock, requestInfo) =>
        {
            //从客户端收到信息
            string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
            client.Logger.Info($"已从{client.ID}接收到信息：{mes}");

            client.Send(mes);//将收到的信息直接返回给发送方

            //client.Send("id",mes);//将收到的信息返回给特定ID的客户端

            var ids = service.GetIDs();
            foreach (var clientId in ids)//将收到的信息返回给在线的所有客户端。
            {
                if (clientId != client.ID)//不给自己发
                {
                    service.Send(clientId, mes);
                }
            }
        };

        service.Setup(new TouchSocketConfig()//载入配置     
            .SetListenIPHosts(new IPHost[] { new IPHost(5000) })//同时监听两个地址
            .ConfigureContainer(a =>//容器的配置顺序应该在最前面
            {
                a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
            })
            .ConfigurePlugins(a =>
            {
                //a.Add();//此处可以添加插件
            }))
            .Start();//启动
        Console.ReadKey();
    }
}
public class ApiServer : RpcServer
{
    [Router("[api]/[action]ab")]//此路由会以"/Server/Sumab"实现
    [WebApi(HttpMethodType.GET)]
    public int Sum(int a, int b)
    {
        return a + b;
    }

    [WebApi(HttpMethodType.POST)]
    public int TestPost(MyClass myClass)
    {
        return myClass.A + myClass.B;
    }

    /// <summary>
    /// 使用调用上下文，响应文件下载。
    /// </summary>
    /// <param name="callContext"></param>
    [WebApi(HttpMethodType.GET, MethodFlags = MethodFlags.IncludeCallContext)]
    public Task<string> DownloadFile(IWebApiCallContext callContext, string id)
    {
        if (id == "rrqm")
        {
            callContext.HttpContext.Response.FromFile(@"D:\System\Windows.iso", callContext.HttpContext.Request);
            return Task.FromResult("ok");
        }
        return Task.FromResult("id不正确。");
    }

    /// <summary>
    /// 使用调用上下文，获取实际请求体。
    /// </summary>
    /// <param name="callContext"></param>
    [WebApi(HttpMethodType.POST, MethodFlags = MethodFlags.IncludeCallContext)]//声明包含调用上下文
    [Router("[api]/[action]")]
    public Task<string> PostContent(IWebApiCallContext callContext)
    {
        if (callContext.Caller is ISocketClient socketClient)
        {
            //this.m_logger.Info($"IP:{socketClient.IP},Port:{socketClient.Port}");//获取Ip和端口
        }
        if (callContext.HttpContext.Request.TryGetContent(out byte[] content))
        {
            //this.m_logger.Info($"共计：{content.Length}");
        }

        return Task.FromResult("ok");
    }
}

public class MyClass
{
    public int A { get; set; }
    public int B { get; set; }
}
