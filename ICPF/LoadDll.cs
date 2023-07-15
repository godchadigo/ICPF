using ICPFCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PluginFramework
{
    /// <summary>
    /// dll文件的加载
    /// </summary>
    public class LoadDll
    {
        /// <summary>
        /// 任务实体
        /// </summary>
        public IPlugin _task;
        public Thread _thread;
        /// <summary>
        /// 核心程序集加载
        /// </summary>
        public AssemblyLoadContext _AssemblyLoadContext { get; set; }
        /// <summary>
        /// 获取程序集
        /// </summary>
        public Assembly _Assembly { get; set; }
        /// <summary>
        /// 文件地址
        /// </summary>
        public string filepath = string.Empty;
        /// <summary>
        /// 文件資料夾路徑
        /// </summary>
        public string dirpath = string.Empty;
        /// <summary>
        /// 指定位置的插件库集合
        /// </summary>
        AssemblyDependencyResolver resolver { get; set; }

        public bool LoadFile(string filepath)
        {
            if (filepath == null) return false;
            this.filepath = filepath;
            this.dirpath = Path.GetDirectoryName(filepath);
            try
            {
                resolver = new AssemblyDependencyResolver(filepath);
                _AssemblyLoadContext = new AssemblyLoadContext(Guid.NewGuid().ToString("N"), true);
                _AssemblyLoadContext.Resolving += _AssemblyLoadContext_Resolving;

                
                using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    var _Assembly = _AssemblyLoadContext.LoadFromStream(fs);
                    var Modules = _Assembly.Modules;
                    //_AssemblyLoadContext.LoadFromAssemblyPath(@"D:\自動控制\專案研究\C#\PluginFramework\PluginFramework\bin\Debug\net6.0\Dependencies\Dapper.dll");
                    foreach (var item in _Assembly.GetTypes())
                    {
                        
                        if (item.GetInterface("IPlugin") != null)
                        {
                            _task = (IPlugin)Activator.CreateInstance(item);
                            break;
                        }
                        if (typeof(PluginBase).IsAssignableFrom(item))
                        {
                            _task = (PluginBase)Activator.CreateInstance(item);
                            // 使用 plugin 对象进行操作
                            break;
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex) { Console.WriteLine($"LoadFile:{ex.Message}"); };
            return false;
        }

        private Assembly _AssemblyLoadContext_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            Console.WriteLine($"加載依賴套件 {arg2.Name}");
            var path = resolver.ResolveAssemblyToPath(arg2);
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            return arg1.LoadFromAssemblyPath(currentDirectory + @"\Dependencies\" + arg2.Name + ".dll");            
        }


        public bool StartTask()
        {
            bool RunState = false;
            try
            {
                if (_task != null)
                {
                    _thread = new Thread(new ThreadStart(_Run));
                    _thread.IsBackground = true;
                    _thread.Start();
                    RunState = true;
                }
            }
            catch (Exception ex) { Console.WriteLine($"StartTask:{ex.Message}"); };
            return RunState;
        }
        
        private void _Run()
        {
            try
            {
                _task.onLoading();
            }
            catch (Exception ex) { Console.WriteLine($"_Run 錯誤:{ex.Message}"); };
        }
        public bool StopTask()
        {
            try
            {
                if (_task != null)
                {
                    _task.onCloseing();
                }                    
                _thread?.Interrupt();
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"UnLoad:{ex.Message}");
            }
            finally
            {
                _thread = null;
            }
            _task = null;
            try
            {
                _AssemblyLoadContext?.Unload();
            }
            catch (Exception)
            { }
            finally
            {
                _AssemblyLoadContext = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return true;
        }
    }
}
