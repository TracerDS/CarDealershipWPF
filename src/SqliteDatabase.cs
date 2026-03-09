using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;

namespace AutoKomis
{
    public class SqliteDatabase
    {
        private readonly string connectionString;
        private readonly string databasePath;

        public string DatabasePath => databasePath;

        public SqliteDatabase(string? databaseFileName = null)
        {
            // Użyj folderu aplikacji jako domyślnej lokalizacji
            string appFolder = AppDomain.CurrentDomain.BaseDirectory;
            databaseFileName ??= "autokomis.db";
            databasePath = Path.Combine(appFolder, databaseFileName);

            connectionString = $"Data Source={databasePath}";

            Console.WriteLine($"Ścieżka do bazy danych: {databasePath}");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                string? directory = Path.GetDirectoryName(databasePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Cars (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        brand TEXT NOT NULL,
                        model TEXT NOT NULL,
                        year INTEGER NOT NULL,
                        mileage INTEGER NOT NULL,
                        fuel TEXT NOT NULL,
                        price REAL NOT NULL,
                        image_data BLOB
                    )";
                command.ExecuteNonQuery();

                Console.WriteLine("Baza danych została utworzona pomyślnie!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas tworzenia bazy danych: {ex.Message}");
                throw;
            }
        }

        public void AddCar(Car car)
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Cars (
                        brand, model, year, mileage, fuel, price, image_data
                    ) VALUES (
                        $brand, $model, $year, $mileage, $fuel, $price, $imageData
                    )";

                command.Parameters.AddWithValue("$brand", car.Brand);
                command.Parameters.AddWithValue("$model", car.Model);
                command.Parameters.AddWithValue("$year", car.Year);
                command.Parameters.AddWithValue("$mileage", car.Mileage);
                command.Parameters.AddWithValue("$fuel", car.Fuel);
                command.Parameters.AddWithValue("$price", car.Price);
                command.Parameters.AddWithValue("$imageData", car.ImageData ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
                Console.WriteLine($"Dodano pojazd: {car.Brand} {car.Model}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas dodawania pojazdu: {ex.Message}");
                throw;
            }
        }

        public List<Car> GetAllCars()
        {
            var cars = new List<Car>();

            try
            {
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT
                        id, brand, model, year, mileage,
                        fuel, price, image_data
                    FROM Cars";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var car = new Car
                    {
                        Brand = reader.GetString(1),
                        Model = reader.GetString(2),
                        Year = reader.GetInt32(3),
                        Mileage = (uint)reader.GetInt32(4),
                        Fuel = reader.GetString(5),
                        Price = reader.GetDecimal(6),
                        ImageData = reader.IsDBNull(7) ? null : reader.GetFieldValue<byte[]>(7)
                    };
                    cars.Add(car);
                }

                Console.WriteLine($"Wczytano {cars.Count} pojazdów z bazy danych");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas wczytywania pojazdów: {ex.Message}");
                throw;
            }

            return cars;
        }

        public void UpdateCar(Car car)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Cars 
                SET
                    brand = $brand,
                    model = $model,
                    year = $year, 
                    mileage = $mileage,
                    fuel = $fuel,
                    price = $price,
                    image_data = $imageData
                WHERE
                    brand = $brand AND model = $model";

            command.Parameters.AddWithValue("$brand", car.Brand);
            command.Parameters.AddWithValue("$model", car.Model);
            command.Parameters.AddWithValue("$year", car.Year);
            command.Parameters.AddWithValue("$mileage", car.Mileage);
            command.Parameters.AddWithValue("$fuel", car.Fuel);
            command.Parameters.AddWithValue("$price", car.Price);
            command.Parameters.AddWithValue("$imageData", car.ImageData ?? (object)DBNull.Value);

            command.ExecuteNonQuery();
        }

        public void DeleteCar(string brand, string model)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM Cars
                WHERE brand = $brand AND model = $model";
            command.Parameters.AddWithValue("$brand", brand);
            command.Parameters.AddWithValue("$model", model);

            command.ExecuteNonQuery();
        }

        public void ClearAllCars()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Cars";
            command.ExecuteNonQuery();
        }
    }
}
