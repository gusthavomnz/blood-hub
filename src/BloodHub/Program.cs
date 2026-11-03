using BloodHub.Data;
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

Console.WriteLine("Bem-vindo ao Blood Hub! Digite 'sair' para encerrar.");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (input is null || input.Trim().Equals("sair", StringComparison.OrdinalIgnoreCase))
        break;
}

return 0;
