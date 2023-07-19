using Dapper;
using DapperExtensions;
using ICPFCore;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using static Dapper.SqlMapper;

namespace Plugin.DatabaseUploader
{
    public class DatabaseUploaderMain : PluginBase
    {
        public override string PluginName => "DatabaseUploader";        
        public override void onLoading() 
        {
            base.onLoading();
            SqlTask();
           
        }
        public void SqlTask()
        {
            Task.Run(async() =>
            {
                while (true)
                {
                    string server = "192.168.0.104";
                    string user = "root";
                    string password = "root";
                    string database = "qj";

                    string connectionString = $"Server={server};Database={database};Uid={user};Pwd={password};";

                    using (MySqlConnection cn = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            cn.Open();

                            var Temp1 = await GetTag("MBUS_2", "1F溫度表_溫度");
                            if (Temp1.IsOk)
                            {
                                var entity = new PluginUploader
                                {
                                    Uuid = Guid.NewGuid().ToString(),
                                    TagName = "MBUS2_Temp",
                                    Message = "Success",
                                    Data = Temp1.Data[0]?.ToString(),
                                    DateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                };

                                string insertQuery = "INSERT INTO pluginuploader (Uuid, TagName , Message , Data ,DateTime) VALUES (@Uuid, @TagName, @Message, @Data,@DateTime)";
                                cn.Execute(insertQuery, entity);

                                Console.WriteLine("向資料庫插入了一筆資料");
                            }
                               
                            cn.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    await Task.Delay(1000);
                }
            });
        }
        public override void onCloseing() 
        {
            base.onCloseing();
        }        
    }
    #region SQL Entity    
    public class PluginUploader
    {
        public string Uuid { get; set; }
        public string TagName { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
        public string DateTime { get; set; }
    }
    #endregion
}