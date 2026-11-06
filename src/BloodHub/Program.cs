using BloodHub.Data;
using BloodHub.Telas;
using MySqlConnector;

var connectionFactory = new ConnectionFactory(DatabaseSettings.FromEnvironment());

try
{
    await using var connection = await connectionFactory.OpenAsync();
    Console.WriteLine($"Conectado ao MySQL {connection.ServerVersion}.");
}
catch (MySqlException ex)
{
    Console.Error.WriteLine($"Falha ao conectar no banco: {ex.Message}");
    return 1;
}

var menu = new MenuPrincipal(connectionFactory);
await menu.ExecutarAsync();

return 0;
