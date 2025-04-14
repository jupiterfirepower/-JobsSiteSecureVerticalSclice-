using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Jobs.Common.Repository;

public class SQLiteGenericRepository
{
    public static void InitDb()
    {
        //var db = new SqliteConnection("Data Source=apikeys;Version=3;New=True;Mode=Memory;Cache=Shared");
        //db.CreateTable<ApiKey>();
        
        using (var connection = new SqliteConnection("Data Source=apikeys;Mode=Memory;Cache=Shared"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"create table ApiKeys (
	                                    Id integer primary key autoincrement,
	                                    ApiKey varchar(64) not null,
                                        Expired datetime,
                                        Created datetime not null default current_timestamp
                                    );";
            var rowsCreated = command.ExecuteNonQuery();
            Console.WriteLine($"Sqlite table created - {rowsCreated}");
            
            var commandInsert = connection.CreateCommand();
            commandInsert.CommandText = @"INSERT INTO ApiKeys (Id,ApiKey,Expired) VALUES (NULL,@ApiKey,@Exprired)";
            //Values added to parameters
            commandInsert.Parameters.AddWithValue("@ApiKey", "1234214124asdfasdfasdfasfd");
            commandInsert.Parameters.AddWithValue("@Exprired", DBNull.Value);
            
            var rowsChanged = commandInsert.ExecuteNonQuery();
            Console.WriteLine($"Insert Rows Changed = {rowsChanged}");
            
            using var commandGetExpired = connection.CreateCommand();
            commandGetExpired.CommandText = "SELECT Expired, Count(*) as Count FROM ApiKeys WHERE ApiKey = @ApiKey;";
            commandGetExpired.Parameters.AddWithValue("@ApiKey", "1234214124asdfasdfasdfasfd");
            //var expired = commandGetExpired.ExecuteScalar();
            string expiredValue = null;
            long countValue = 0;

            using var reader = commandGetExpired.ExecuteReader();
            while (reader.Read())
            {
                if (reader["Expired"].GetType() != typeof(DBNull))
                {
                    expiredValue = reader["Expired"].ToString();
                    countValue = (long)reader["Count"];
                }
                else
                {
                    expiredValue = null;
                    countValue = (long)reader["Count"];
                }
            }
            
            Console.WriteLine($"Sqlite Ok {countValue}");
            if(countValue > 0 && expiredValue == null)
            {
                Console.WriteLine($"Sqlite Ok countValue > 0 && expiredValue == null");
            }
        
            //DateTime expiredDateTime = DateTime.ParseExact(expiredValue, "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
            
            /*var commandInsert = connection.CreateCommand();
            commandInsert.CommandText = @"INSERT INTO ApiKeys (Id,ApiKey,Expired) VALUES (NULL,@ApiKey,@Exprired)";
            //Values added to parameters
            commandInsert.Parameters.AddWithValue("@ApiKey", "1234214124asdfasdfasdfasfd");
            commandInsert.Parameters.AddWithValue("@Exprired", DateTime.UtcNow);
          
            var rowsChanged = commandInsert.ExecuteNonQuery();
            Console.WriteLine($"No of Rows Changed = {rowsChanged}");
            
            var commandFunc = connection.CreateCommand();
            commandFunc.CommandText = @"SELECT Expired FROM ApiKeys WHERE ApiKey = @ApiKey;";
            commandFunc.Parameters.AddWithValue("@ApiKey", "1234214124asdfasdfasdfasfd");
            var stdDev = commandFunc.ExecuteScalar();
            Console.WriteLine($"Sqlite Result : {stdDev!=null}");
            //Console.WriteLine($"Sqlite Result : {stdDev.ToString("yyyyMMdd HH:mm")}");
            //"2024-10-13 05:39:54.3311283"
            string format = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
            Console.WriteLine($"Sqlite Result : {System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern}");
            DateTime myDateTime = DateTime.ParseExact(stdDev.ToString(), "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
            Console.WriteLine($"Sqlite Result : {myDateTime != null}");
            if (myDateTime <= DateTime.UtcNow)
            {
                Console.WriteLine($"Sqlite Result 1 : TRUE");
            }*/
        }
    }
}