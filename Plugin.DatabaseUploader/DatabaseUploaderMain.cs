using ConsolePluginTest;
using Dapper;
using DapperExtensions;
using PluginFramework;
using System.Data.SqlClient;

namespace Plugin.DatabaseUploader
{
    public class DatabaseUploaderMain : PluginBase
    {
        public override string PluginName => "DatabaseUploader";        
        public override void onLoading() 
        {
            base.onLoading();
            string server = "127.0.0.1";
            string user = "sa";
            string password = "Asd279604823";
            string database = "QJ";

            string connectionString = $"Server={server};User Id={user};Password={password};Database={database};";

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                try
                {
                    cn.Open();
                    //int personId = 1;
                    //PluginUploaderModel person = cn.Get<PluginUploaderModel>(personId);
                    //PluginUploader model = new PluginUploader { Uuid = Guid.NewGuid().ToString(), TagName = "D0", Message = "你好", Data = "kk" };
                    var entity = new PluginUploader
                    {
                        Uuid = Guid.NewGuid(),
                        TagName = "Tag1",
                        Message = "Hello, Dapper!",
                        Data = "Some data"
                    };
              
                    cn.Insert(entity);
                    Console.WriteLine("向資料庫插入了一筆資料");
                    cn.Close();
                }
                catch (Exception ex) { }                
            }
        }
        public override void onCloseing() 
        {
            base.onCloseing();
        }        
    }
    #region SQL Entity    
    public class PluginUploader
    {
        public Guid Uuid { get; set; }
        public string TagName { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }
    #endregion
}