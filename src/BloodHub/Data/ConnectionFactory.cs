using MySqlConnector;

namespace BloodHub.Data;

public sealed class ConnectionFactory(DatabaseSettings settings)
{
    private readonly string _connectionString = new MySqlConnectionStringBuilder
    {
        Server = settings.Host,
        Port = settings.Port,
        Database = settings.Database,
        UserID = settings.User,
        Password = settings.Password,
        UseAffectedRows = false,
    }.ConnectionString;

    public async Task<MySqlConnection> OpenAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
