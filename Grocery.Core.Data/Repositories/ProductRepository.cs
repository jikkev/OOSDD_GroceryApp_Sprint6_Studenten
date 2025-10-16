using Grocery.Core.Data;
using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;
using System;


namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        private readonly List<Product> products = [];
        public ProductRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS Product (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [Name] NVARCHAR(80) UNIQUE NOT NULL,
                            [Stock] INTEGER NOT NULL,
                            [ShelfLife] DATE NULL,
                            [Price] REAL NOT NULL)");

            List<string> insertQueries = [
                @"INSERT OR IGNORE INTO Product(Name, Stock, ShelfLife, Price) VALUES('Melk', 300, '2025-09-25', 0.95)",
                @"INSERT OR IGNORE INTO Product(Name, Stock, ShelfLife, Price) VALUES('Kaas', 100, '2025-09-30', 7.98)",
                @"INSERT OR IGNORE INTO Product(Name, Stock, ShelfLife, Price) VALUES('Brood', 400, '2025-09-12', 2.19)",
                @"INSERT OR IGNORE INTO Product(Name, Stock, ShelfLife, Price) VALUES('Cornflakes', 0, '2025-12-31', 1.48)"
            ];

            InsertMultipleWithTransaction(insertQueries);
            GetAll();
        }
        public List<Product> GetAll()
        {
            products.Clear();
            const string selectQuery = "SELECT Id, Name, Stock, ShelfLife, Price FROM Product";

            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly shelfLife = reader.IsDBNull(3)
                        ? default
                        : DateOnly.Parse(reader.GetString(3));
                    decimal price = reader.IsDBNull(4)
                        ? 0m
                        : Convert.ToDecimal(reader.GetDouble(4));

                    products.Add(new(id, name, stock, shelfLife, price));
                }
            }

            CloseConnection();
            return products;
        }

        public Product? Get(int id)
        {
            const string selectQuery = "SELECT Id, Name, Stock, ShelfLife, Price FROM Product WHERE Id = @Id";
            Product? product = null;

            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly shelfLife = reader.IsDBNull(3)
                        ? default
                        : DateOnly.Parse(reader.GetString(3));
                    decimal price = reader.IsDBNull(4)
                        ? 0m
                        : Convert.ToDecimal(reader.GetDouble(4));

                    product = new(id, name, stock, shelfLife, price);
                }
            }

            CloseConnection();
            return product;
        }

        public Product Add(Product item)
        {
            const string insertQuery = @"INSERT INTO Product(Name, Stock, ShelfLife, Price)
                                           VALUES(@Name, @Stock, @ShelfLife, @Price)
                                           RETURNING Id";

            OpenConnection();
            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Stock", item.Stock);
                command.Parameters.AddWithValue("@ShelfLife", item.ShelfLife == default ? null : item.ShelfLife.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@Price", item.Price);

                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }

            CloseConnection();
            return item;
        }

        public Product? Delete(Product item)
        {
            const string deleteQuery = "DELETE FROM Product WHERE Id = @Id";

            OpenConnection();
            using (SqliteCommand command = new(deleteQuery, Connection))
            {
                command.Parameters.AddWithValue("@Id", item.Id);
                command.ExecuteNonQuery();
            }

            CloseConnection();
            return item;
        }

        public Product? Update(Product item)
        {
            const string updateQuery = @"UPDATE Product
                                          SET Name = @Name,
                                              Stock = @Stock,
                                              ShelfLife = @ShelfLife,
                                              Price = @Price
                                          WHERE Id = @Id";

            OpenConnection();
            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Stock", item.Stock);
                command.Parameters.AddWithValue("@ShelfLife", item.ShelfLife == default ? null : item.ShelfLife.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@Price", item.Price);
                command.Parameters.AddWithValue("@Id", item.Id);

                command.ExecuteNonQuery();
            }

            CloseConnection();
            return item;
        }
    }
}
