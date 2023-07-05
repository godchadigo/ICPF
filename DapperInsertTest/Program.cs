using System.Data.SqlClient;
using Dapper;
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
    

        var query = "INSERT INTO PluginUploader (Uuid, TagName, Message, Data) VALUES (@Uuid, @TagName, @Message, @Data)";
        var newId = cn.Insert<Guid, PluginUploader>(new PluginUploader { TagName = "D0", Message = "你好", Data = "kk" });
        Console.WriteLine("向資料庫插入了一筆資料");
        cn.Close();
    }
    catch (Exception ex) { }
}

#region SQL Entity   
[Table("PluginUploader")]
public class PluginUploader
{
    [Key]
    public Guid Uuid { get; set; }
    public string TagName { get; set; }
    public string Message { get; set; }
    public string Data { get; set; }
}
#endregion