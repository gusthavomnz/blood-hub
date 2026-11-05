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
    Console.Error.WriteLine(TradutorErroBanco.Traduzir(ex, "conectar ao banco").Message);
    return 1;
}

var repositorio = new DoadorRepositorio(connectionFactory);
await new TelaDoador(repositorio).ExecutarAsync();

return 0;
