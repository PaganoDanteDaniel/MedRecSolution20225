using MySql.Data.MySqlClient;

namespace Test;

internal class Program
{
    static void Main(string[] args)
    {
        var conn = new MySqlConnection("Server=DantePc;Port=3306;Database=medrecdb;User=appuser;Password=MiPass123!;AllowPublicKeyRetrieval=True;SslMode=none");
        conn.Open();
        Console.WriteLine(conn.ServerVersion);
        conn.Close();
        Console.ReadLine();
    }
}
