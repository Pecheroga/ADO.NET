using System;
using System.Data;
using System.Data.SqlClient;

namespace ConnectedLayer
{
    public class CustomersDb: BaseDb
    {
        public void ProcessCredirRisk(bool throwEx, int custId)
        {
            string fName;
            string lName;

            const string querySelect = "SELECT * FROM Customers WHERE Id = @id";
            using (var commandSelect = new SqlCommand(querySelect, Connection))
            {
                var parameter = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = custId,
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Input
                };
                commandSelect.Parameters.Add(parameter);
                using (var dataReader = commandSelect.ExecuteReader())
                {
                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        fName = (string)dataReader["FirstName"];
                        lName = (string)dataReader["LastName"];
                    }
                    else
                    {
                        return;
                    }
                }    
            }

            const string queryDelete =
                "DELETE FROM Customers WHERE ID = @custId";
            const string queryInsert =
                "INSERT INTO CreditRisks (Id, FirstName, LastName) VALUES (@custId, @fName, @lName)";

            using (var commandDelete = new SqlCommand(queryDelete, Connection))
            using (var commandInsert = new SqlCommand(queryInsert, Connection))
            using (var transaction = Connection.BeginTransaction())
            {
                var param = new SqlParameter
                {
                    ParameterName = "@custId",
                    Value = custId,
                    SqlDbType = SqlDbType.Int
                };
                commandDelete.Parameters.Add(param);

                param = new SqlParameter
                {
                    ParameterName = "@custId",
                    Value = custId,
                    SqlDbType = SqlDbType.Int
                };
                commandInsert.Parameters.Add(param);

                param = new SqlParameter
                {
                    ParameterName = "@fName",
                    Value = fName,
                    SqlDbType = SqlDbType.Char
                };
                commandInsert.Parameters.Add(param);

                param = new SqlParameter
                {
                    ParameterName = "@lName",
                    Value = lName,
                    SqlDbType = SqlDbType.Char
                };
                commandInsert.Parameters.Add(param);

                try
                {
                    commandDelete.Transaction = transaction;
                    commandInsert.Transaction = transaction;
                    commandDelete.ExecuteNonQuery();
                    commandInsert.ExecuteNonQuery();
                    if (throwEx)
                    {
                        throw new ApplicationException("Data Base error. Transaction have been fail");
                    }
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    transaction.Rollback();
                }
            }
        }
    }
}
