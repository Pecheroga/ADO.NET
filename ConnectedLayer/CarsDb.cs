using System;
using System.Data;
using System.Data.SqlClient;

namespace ConnectedLayer
{
    public class CarsDb: BaseDb
    {
        public void InsertAuto(int carId, string name, string color, string make, string petName)
        {
            const string query = "INSERT INTO Inventory (Id, Name, Color, Make, PetName) " +
                                 "VALUES (@id, @name, @color, @make, @petName)";
            using (var command = new SqlCommand(query, Connection))
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter
                    {
                        ParameterName = "@id",
                        Value = carId,
                        SqlDbType = SqlDbType.Int
                    },
                    new SqlParameter
                    {
                        ParameterName = "@name",
                        Value = name,
                        SqlDbType = SqlDbType.Char,
                        Size = 10
                    },
                    new SqlParameter
                    {
                        ParameterName = "@color",
                        Value = color,
                        SqlDbType = SqlDbType.Char,
                        Size = 10
                    }, 
                    new SqlParameter
                    {
                        ParameterName = "@make",
                        Value = make,
                        SqlDbType = SqlDbType.Char,
                        Size = 10
                    },
                    new SqlParameter
                    {
                        ParameterName = "@petName",
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
            const string query = "DELETE FROM Inventory WHERE Id = @id";
            using (var command = new SqlCommand(query, Connection))
            {
                var parameter = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = carId,
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(parameter);
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
            const string query = "UPDATE Inventory SET PetName = @petName WHERE Id = @id";
            using (var command = new SqlCommand(query, Connection))
            {
                var parameters = new[]
                {
                    new SqlParameter
                    {
                        ParameterName = "@petName",
                        Value = newPetName,
                        SqlDbType = SqlDbType.Char,
                        Direction = ParameterDirection.Input
                    }, 
                    new SqlParameter
                    {
                        ParameterName = "@id",
                        Value = carId,
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Input
                    }
                };
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }
        
        public string GetPetNameProcedure(int carId)
        {
            string petName;
            using (var command = new SqlCommand("GetPetName", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                var parameters = new[]
                {
                    new SqlParameter
                    {
                        ParameterName = "@id",
                        Value = carId,
                        SqlDbType= SqlDbType.Int,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@petName",
                        Size = 10,
                        SqlDbType = SqlDbType.Char,
                        Direction = ParameterDirection.Output
                    }
                };
                command.Parameters.AddRange(parameters);

                command.ExecuteNonQuery();
                petName = (string) command.Parameters["@petName"].Value;
                petName = petName.Trim();
            }
            return petName;
        }
    }
}

