using System.Reflection;

namespace ConsolePluginTest
{
    public interface IPlugin
    {
        void onLoading();
        void onCloseing();

        //event EventHandler Loading;
    }



    class Program
    {
        
        static void Main(string[] args)
        {
            Program p = new Program();
            List<string> pluginpath = p.FindPlugin();
            List<IPlugin> plugins = new List<IPlugin>();            

            pluginpath = p.DeleteInvalidPlungin(pluginpath);

            foreach (string filename in pluginpath)
            {
                try
                {
                    //获取文件名
                    string asmfile = filename;
                    string asmname = Path.GetFileNameWithoutExtension(asmfile);
                    if (asmname != string.Empty)
                    {
                        // 利用反射,构造DLL文件的实例
                        Assembly asm = Assembly.LoadFile(asmfile);
                        //利用反射,从程序集(DLL)中,提取类,并把此类实例化
                        Type[] t = asm.GetExportedTypes();
                        foreach (Type type in t)
                        {
                            if (type.GetInterface("IPlugin") != null)
                            {
                                IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                                plugin.onLoading();
                                plugins.Add(plugin);
                                // 订阅 Loading 事件
                                //plugin.Loading += Core_MyEvent1;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }      

            Console.ReadKey();

            //通知插件關閉
            foreach (var plugin in plugins)
            {
                plugin.onCloseing();
            }            
        }



        //查找所有插件的路径
        private List<string> FindPlugin()
        {
            List<string> pluginpath = new List<string>();
            try
            {
                //获取程序的基目录
                string path = AppDomain.CurrentDomain.BaseDirectory;
                //合并路径，指向插件所在目录。
                path = Path.Combine(path, "Plugins");
                foreach (string filename in Directory.GetFiles(path, "*.dll"))
                {
                    pluginpath.Add(filename);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return pluginpath;
        }
        //载入插件，在Assembly中查找类型
        private object LoadObject(Assembly asm, string className, string interfacename
                        , object[] param)
        {
            try
            {
                //取得className的类型
                Type t = asm.GetType(className);
                if (t == null
                    || !t.IsClass
                    || !t.IsPublic
                    || t.IsAbstract
                    || t.GetInterface(interfacename) == null
                   )
                {
                    return null;
                }
                //创建对象
                Object o = Activator.CreateInstance(t, param);
                if (o == null)
                {
                    //创建失败，返回null
                    return null;
                }
                return o;
            }
            catch
            {
                return null;
            }
        }
        //移除无效的的插件，返回正确的插件路径列表，Invalid:无效的
        private List<string> DeleteInvalidPlungin(List<string> PlunginPath)
        {
            string interfacename = typeof(IPlugin).FullName;
            List<string> rightPluginPath = new List<string>();
            //遍历所有插件。
            foreach (string filename in PlunginPath)
            {
                try
                {
                    Assembly asm = Assembly.LoadFile(filename);
                    //遍历导出插件的类。
                    foreach (Type t in asm.GetExportedTypes())
                    {
                        //查找指定接口
                        Object plugin = LoadObject(asm, t.FullName, interfacename, null);
                        //如果找到，将插件路径添加到rightPluginPath列表里，并结束循环。
                        if (plugin != null)
                        {
                            rightPluginPath.Add(filename);
                            break;
                        }
                    }
                }
                catch
                {
                    //throw new Exception(filename + "不是有效插件");
                    Console.WriteLine(filename + "不是有效插件");
                }
            }
            return rightPluginPath;
        }        
        private static void Core_MyEvent1(object sender, EventArgs e)
        {
            Console.WriteLine("Event triggered!");
        }
    }   
    public class MemoryShareManager
    {
        public static MemoryShareManager instance = new MemoryShareManager();
        public int Data { get; set; }
    }
}