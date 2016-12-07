using System;
using System.Data;
using System.Data.SqlClient;

namespace ConnectedLayer
{
    public class Cars
    {
        public SqlConnection Connection { get; private set; }

        public void OpenConnection(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
            Connection.Open();
        }

        public void CloseConnection()
        {
            Connection.Close();
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

        
        public void InsertAuto(int carId, string name, string color, string make, string petName)
        {
            var query = "INSERT INTO Inventory (Id, Name, Color, Make, PetName) " +
                        "VALUES (@CarId, @Name, @Color, @Make, @PetName)";
            using (var command = new SqlCommand(query, Connection))
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter
                    {
                        ParameterName = "@CarId",
                        Value = carId,
                        SqlDbType = SqlDbType.Int
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = name,
                        SqlDbType = SqlDbType.Char,
                        Size = 10
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Color",
                        Value = color,
                        SqlDbType = SqlDbType.Char,
                        Size = 10
                    }, 
                    new SqlParameter
                    {
                        ParameterName = "@Make",
                        Value = make,
                        SqlDbType = SqlDbType.Char,
                        Size = 10
                    },
                    new SqlParameter
                    {
                        ParameterName = "@PetName",
                        Value = petName,
                        SqlDbType = SqlDbType.Char,
                        Size = 10
                    }
                };
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteAuto(int carId)
        {
            var query = string.Format("DELETE FROM Inventory " +
                                      "WHERE Id = '{0}'", carId);
            using (var command = new SqlCommand(query, Connection))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    var errorException = new Exception("Can't delete, because the car is ordered", exception);
                    throw errorException;
                }
            }
        }

        public void UpdateAutoPetName(int carId, string newPetName)
        {
            var query = string.Format("UPDATE Inventory SET PetName= '{0}' " +
                                      "WHERE Id = '{1}'", newPetName, carId);
            using (var command = new SqlCommand(query, Connection))
            {
                command.ExecuteNonQuery();
            }
        }
        
        public string GetPetNameProcedure(int carId)
        {
            string petName;
            using (var command = new SqlCommand("GetPetName", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                var param = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = carId,
                    SqlDbType= SqlDbType.Int,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(param);

                param = new SqlParameter
                {
                    ParameterName = "@petName",
                    Size = 10,
                    SqlDbType = SqlDbType.Char,
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(param);

                command.ExecuteNonQuery();
                petName = (string) command.Parameters["@petName"].Value;
                petName = petName.Trim();
            }
            return petName;
        }

        public void ProcessCredirRisk(bool throwEx, int custId)
        {
            string fName;
            string lName;

            var querySelect = "SELECT * FROM Customers WHERE Id = " + custId;
            using (var commandSelect = new SqlCommand(querySelect, Connection))
            using (var dataReader = commandSelect.ExecuteReader())
            {
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    fName = (string) dataReader["FirstName"];
                    lName = (string) dataReader["LastName"];
                }
                else
                {
                    return;
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

