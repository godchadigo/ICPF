using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Configuration;

namespace OpcuaServer
{
    internal class OpcuaServerMain
    {
        static void Main(string[] args)
        {
            // 创建一个新的OPC UA服务器实例
            var server = new StandardServer();

            try
            {
                // 启动服务器
                server.Start();

                // 添加自定义的节点
                var rootNode = server.DefaultSystemContext.NodeCache.Find(Objects.ObjectTypesFolder, false, null, null) as FolderState;
                var customNode = new BaseObjectState(rootNode);
                customNode.SymbolicName = "CustomNode";
                customNode.TypeDefinitionId = ObjectTypeIds.BaseObjectType;
                customNode.NodeId = new NodeId("ns=2;s=CustomNode");

                rootNode.AddReference(new NodeStateReference(ReferenceTypes.Organizes, false, customNode.NodeId));

                // 注册自定义节点到服务器地址空间
                server.DefaultSystemContext.NodeCache.UpdateNode(server.DefaultSystemContext, customNode);

                Console.WriteLine("OPC UA服务器已启动。按任意键停止服务器...");
                Console.ReadKey();
            }
            finally
            {
                // 停止服务器
                server.Stop();
            }
        }
    }
}