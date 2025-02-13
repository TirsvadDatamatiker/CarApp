﻿using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace CarApp
{
    public class DbSqliteHandler
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbSqliteHandler"/> class.
        /// </summary>
        /// <param name="dbPath">The path to the SQLite database file.</param>
        public DbSqliteHandler(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase(dbPath);
        }

        /// <summary>
        /// Initializes the database by creating it if it doesn't exist and applying migrations.
        /// </summary>
        /// <param name="dbPath">The path to the SQLite database file.</param>
        private void InitializeDatabase(string dbPath)
        {
            // Create database if it doesn't exist
            if (!File.Exists(dbPath))
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    CreateDb(connection);
                }
            }

            // Apply migrations
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                CreateMigrationVersionTable(connection);
                ApplyMigrations(connection);
            }
        }

        /// <summary>
        /// Creates the database using the SQL script specified in the Globals.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        private static void CreateDb(IDbConnection connection)
        {
            var sql = File.ReadAllText(Globals.DbSqliteCreateDbFileName);
            connection.Execute(sql);
        }

        /// <summary>
        /// Creates the MigrationVersion table if it doesn't exist.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        private static void CreateMigrationVersionTable(IDbConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS MigrationVersion (
                    Id INTEGER PRIMARY KEY,
                    Version INTEGER NOT NULL
                );
                INSERT INTO MigrationVersion (Id, Version)
                SELECT 1, 0
                WHERE NOT EXISTS (SELECT 1 FROM MigrationVersion WHERE Id = 1);
            ";
            connection.Execute(sql);
        }

        /// <summary>
        /// Gets the current version of the database.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <returns>The current version of the database.</returns>
        private static int GetCurrentVersion(IDbConnection connection)
        {
            var sql = "SELECT Version FROM MigrationVersion WHERE Id = 1";
            return connection.QuerySingle<int>(sql);
        }

        /// <summary>
        /// Updates the current version of the database.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="version">The new version of the database.</param>
        private static void UpdateCurrentVersion(IDbConnection connection, int version)
        {
            var sql = "UPDATE MigrationVersion SET Version = @Version WHERE Id = 1";
            connection.Execute(sql, new { Version = version });
        }

        /// <summary>
        /// Applies the necessary migrations to the database.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        private void ApplyMigrations(IDbConnection connection)
        {
            var currentVersion = GetCurrentVersion(connection);
            var migrationFiles = GetMigrationFiles();

            foreach (var file in migrationFiles)
            {
                var version = int.Parse(Path.GetFileNameWithoutExtension(file).Split('.').Last());
                if (version > currentVersion)
                {
                    var sql = File.ReadAllText(file);
                    connection.Execute(sql);
                    UpdateCurrentVersion(connection, version);
                }
            }
        }

        /// <summary>
        /// Gets the list of migration files.
        /// </summary>
        /// <returns>The list of migration files.</returns>
        private IEnumerable<string> GetMigrationFiles()
        {
            var migrationFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "CreateDatabase.*.sql")
                                          .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f).Split('.').Last()))
                                          .ToList();
            return migrationFiles;
        }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        public IDbConnection Connection => new SqliteConnection(_connectionString);

        /// <summary>
        /// Gets the list of cars from the database.
        /// </summary>
        /// <returns>The list of cars.</returns>
        public IEnumerable<Car> GetCars()
        {
            using (var connection = Connection)
            {
                return connection.Query<Car>("SELECT * FROM Cars");
            }
        }

        /// <summary>
        /// Gets the list of fuel types from the database.
        /// </summary>
        /// <returns>The list of fuel types.</returns>
        public IEnumerable<FuelType> GetFuelTypes()
        {
            using (var connection = Connection)
            {
                return connection.Query<FuelType>("SELECT * FROM FuelTypes ORDER BY Name");
            }
        }

        /// <summary>
        /// Gets a fuel type by its ID.
        /// </summary>
        /// <param name="id">The ID of the fuel type.</param>
        /// <returns>The fuel type with the specified ID.</returns>
        public FuelType? GetFuelType(int id)
        {
            using (var connection = Connection)
            {
                return connection.QueryFirstOrDefault<FuelType>("SELECT * FROM FuelTypes WHERE Id = @Id", new { Id = id });
            }
        }

        /// <summary>
        /// Adds a new car to the database.
        /// </summary>
        /// <param name="car">The car to add.</param>
        public void AddCar(Car car)
        {
            using (var connection = Connection)
            {
                var sql = "INSERT INTO Cars (Brand, Model, Year, GearType, FuelTypeId, FuelEfficiency, Mileage, Description) " +
                          "VALUES (@Brand, @Model, @Year, @GearType, @FuelTypeId, @FuelEfficiency, @Mileage, @Description)";
                connection.Execute(sql, car);
            }
        }

        /// <summary>
        /// Updates the details of an existing car in the database.
        /// </summary>
        /// <param name="car">The car object containing updated details.</param>
        public void UpdateCar(Car car)
        {
            using (var connection = Connection)
            {
                var sql = "UPDATE Cars SET Brand = @Brand, Model = @Model, Year = @Year, GearType = @GearType, " +
                          "FuelTypeId = @FuelTypeId, FuelEfficiency = @FuelEfficiency, Mileage = @Mileage, Description = @Description " +
                          "WHERE Id = @Id";
                connection.Execute(sql, car);
            }
        }

        /// <summary>
        /// Adds a new fuel type to the database.
        /// </summary>
        /// <param name="fuelType">The fuel type to add.</param>
        public void AddFuelType(FuelType fuelType)
        {
            using (var connection = Connection)
            {
                var sql = "INSERT INTO FuelTypes (Name, Price) VALUES (@Name, @Price)";
                connection.Execute(sql, fuelType);
            }
        }
    }
}
