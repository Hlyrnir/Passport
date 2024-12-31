using Dapper;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Passport.Infrastructure.Test.DataAccess
{
    public class SqliteDataAccessSpecification : IDisposable
    {
        private readonly IConfiguration cfgConfiguration;

        private const string sTableName = "TEST_SqliteDataAccessSpecification";
        private const string sColumnName = "TEST_Context";

        public SqliteDataAccessSpecification()
        {
            cfgConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new[]
                    {
                        new KeyValuePair<string, string?>("ConnectionStrings:ValidConnectionString", "Data Source=D:\\Dateien\\Projekte\\CSharp\\CQRS_Prototype\\TEST_Passport.db; Mode=ReadWrite"),
                        new KeyValuePair<string, string?>("ConnectionStrings:InvalidConnectionString", "Data Source=INVALID_DATABASE.db; Mode=ReadWrite")
                    })
                .Build();
        }

        [Fact]
        public async Task Connection_DatabaseShouldNotBeEmpty_WhenConnectionStringIsValid()
        {
            // Arrange
            SqliteDataAccess sqlDataAccess = new SqliteDataAccess(cfgConfiguration, "ValidConnectionString");
            IEnumerable<string> enumTable = Enumerable.Empty<string>();

            // Act
            using (IDbConnection sqlConnection = sqlDataAccess.Connection)
            {
                enumTable = await sqlConnection.QueryAsync<string>("SELECT name FROM sqlite_schema WHERE type ='table' AND name NOT LIKE 'sqlite_%';");
            }

            // Assert
            enumTable.Should().NotBeNullOrEmpty();
            sqlDataAccess.Connection.State.Should().Be(ConnectionState.Closed);
        }

        [Fact]
        public async Task Connection_DatabaseShouldBeEmpty_WhenConnectionStringIsInvalid()
        {
            // Arrange
            SqliteDataAccess sqlDataAccess = new SqliteDataAccess(cfgConfiguration, "InvalidConnectionString");
            IEnumerable<string> enumTable = Enumerable.Empty<string>();

            // Act
            Func<Task> result = async () =>
            {
                using (IDbConnection sqlConnection = sqlDataAccess.Connection)
                {
                    enumTable = await sqlConnection.QueryAsync<string>("SELECT name FROM sqlite_schema WHERE type ='table' AND name NOT LIKE 'sqlite_%';");
                }
            };

            // Assert
            await result.Should().ThrowExactlyAsync<SqliteException>();

            enumTable.Should().BeEmpty();
            sqlDataAccess.Connection.State.Should().Be(ConnectionState.Closed);
        }

        [Fact]
        public async Task Transaction_ShouldCreateTable_WhenTransactionIsCommited()
        {
            // Arrange
            SqliteDataAccess sqlDataAccess = new SqliteDataAccess(cfgConfiguration, "ValidConnectionString");

            bool bIsCommited = false;
            IEnumerable<string> enumTableName = Enumerable.Empty<string>();

            // Act
            await sqlDataAccess.TransactionAsync(async () =>
            {
                await sqlDataAccess.Connection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS {sTableName} ({sColumnName} TEXT NOT NULL);");

                sqlDataAccess.TryCommit();
                bIsCommited = true;
            });

            enumTableName = await sqlDataAccess.Connection.QueryAsync<string>($"SELECT name FROM sqlite_master WHERE type='table' AND name='{sTableName}';");

            // Assert
            bIsCommited.Should().BeTrue();
            enumTableName.Should().Contain(sTableName);
            sqlDataAccess.Connection.State.Should().Be(ConnectionState.Closed);
        }

        [Fact]
        public async Task Transaction_ShouldNotCreateTable_WhenTransactionIsRolledBack()
        {
            // Arrange
            SqliteDataAccess sqlDataAccess = new SqliteDataAccess(cfgConfiguration, "ValidConnectionString");

            bool bIsRolledBack = false;
            IEnumerable<string> enumTableName = Enumerable.Empty<string>();

            // Act
            await sqlDataAccess.TransactionAsync(async () =>
            {
                await sqlDataAccess.Connection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS {sTableName} ({sColumnName} TEXT NOT NULL);");

                sqlDataAccess.TryRollback();
                bIsRolledBack = true;
            });

            enumTableName = await sqlDataAccess.Connection.QueryAsync<string>($"SELECT name FROM sqlite_master WHERE type='table' AND name='{sTableName}';");

            // Assert
            bIsRolledBack.Should().BeTrue();
            enumTableName.Should().NotContain(sTableName);
            sqlDataAccess.Connection.State.Should().Be(ConnectionState.Closed);
        }

        [Fact]
        public async Task Transaction_ShouldNotCreateTable_WhenTransactionIsAborted()
        {
            // Arrange
            SqliteDataAccess sqlDataAccess = new SqliteDataAccess(cfgConfiguration, "ValidConnectionString");

            // Act
            await sqlDataAccess.TransactionAsync(async () =>
            {
                await sqlDataAccess.Connection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS {sTableName} ({sColumnName} TEXT NOT NULL);");
            });

            // Assert
            sqlDataAccess.Connection.State.Should().Be(ConnectionState.Closed);
        }

        public async void Dispose()
        {
            SqliteDataAccess sqlDataAccess = new SqliteDataAccess(cfgConfiguration, "ValidConnectionString");

            using (IDbConnection sqlConnection = sqlDataAccess.Connection)
            {
                await sqlConnection.ExecuteAsync($"DROP TABLE IF EXISTS {sTableName};");
            }
        }
    }
}
