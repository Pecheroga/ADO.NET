using System.Data;
using System.Data.SqlClient;

namespace ConnectedLayer
{
    public class BaseDb
    {
        public SqlConnection Connection { get; private set; }

        public void OpenConnection(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
            Connection.Open();
        }

        public void CloseConnection()
        {
            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();    
            }
        }

        public bool IsPresentId(string tableDb, int id)
        {
            var query = string.Format("SELECT * FROM {0} WHERE Id = @Id", tableDb);
            using (var command = new SqlCommand(query, Connection))
            {
                var param = new SqlParameter
                {
                    ParameterName = "@Id",
                    Value = id,
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(param);

                using (var reader = command.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }

        public DataTable GetAllFrom(string tableDb)
        {
            var dataTable = new DataTable();
            var query = string.Format("SELECT * FROM {0}", tableDb);
            using (var command = new SqlCommand(query, Connection))
            {
                var dataReader = command.ExecuteReader();
                dataTable.Load(dataReader);
                dataReader.Close();
            }
            return dataTable;
        }
    }
}
