using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using Dapper;
using DapperExtensions;
using MySql.Data.MySqlClient;

string server = "192.168.0.104";
string user = "root";
string password = "root";
string database = "qj";

string connectionString = $"Server={server};Database={database};Uid={user};Pwd={password};";

using (MySqlConnection cn = new MySqlConnection("server=127.0.0.1;database=qj;uid=root;pwd=root;charset='utf8';SslMode=None"))
{
    try
    {
        cn.Open();
        //int personId = 1;
        //PluginUploaderModel person = cn.Get<PluginUploaderModel>(personId);

        /*
        var query = "INSERT INTO PluginUploader (Uuid, TagName, Message, Data) VALUES (@Uuid, @TagName, @Message, @Data)";
        var uuid = Guid.NewGuid().ToString();
        var data = new PluginUploader() { Uuid = "111", TagName = "a", Message = "a", Data = "a" };
        var newId = cn.Insert(data);
        Console.WriteLine("向資料庫插入了一筆資料");
        */
        // 執行查詢並映射結果到Customer類別的集合
        /*
        string query = "SELECT * FROM PluginUploader";
        IEnumerable<PluginUploader> customers = cn.Query<PluginUploader>(query);

        // 使用查詢結果
        foreach (PluginUploader customer in customers)
        {
            Console.WriteLine($"ID: {customer.Uuid}, Name: {customer.TagName}, Email: {customer.Message}");
        }
        */
        var data = new pluginuploader() { Uuid = Guid.NewGuid().ToString() , Name = "a" };
        string insertQuery = "INSERT INTO pluginuploader (Uuid, Name) VALUES (@Uuid, @Name)";
        cn.Execute(insertQuery, data);
        cn.Close();
    }
    catch (Exception ex) 
    {
        Console.WriteLine(ex.Message);
    }
}

#region SQL Entity   
[Table("pluginuploader")]
public class pluginuploader
{
    [Key]
    public string Uuid { get; set; }
    public string Name { get; set; }
}
#endregion