using System;
using System.Data.SqlClient;

namespace Tasks_Server
{
    static class SQL
    {
        static public object[] getData(string Command)
        {
            SqlDataReader data;
            SqlConnection con = new SqlConnection(Program.MainConnectionString);
            con.Open();
            SqlCommand com = new SqlCommand();
            com.CommandText = Command;
            com.Connection = con;
            data = com.ExecuteReader();

            object[] result = new object[0], tmp = new object[data.FieldCount];
            while (data.Read())
            {
                tmp = new object[data.FieldCount];
                data.GetValues(tmp);
                result = inserLine(result, tmp);
            }

            con.Close();

            //System.Threading.Thread.Sleep(1000);

            return result;
        }
    
        static private object[] inserLine(object[] List, object[] Values)
        {
            object[] result = new object[List.Length + 1];
            List.CopyTo(result, 0);
            result[List.Length] = Values;
            return result;
        }
    }
}
