using BloodHub.Data;
using BloodHub.Telas;
using BloodHub.Validacao;
using MySqlConnector;

var connectionFactory = new ConnectionFactory(DatabaseSettings.FromEnvironment());

try
{
    await using var connection = await connectionFactory.OpenAsync();
    Console.WriteLine($"Conectado ao MySQL {connection.ServerVersion}.");
}
catch (MySqlException ex)
{
    var mensagem = TradutorErroBanco.Traduzir(ex);
    Console.Error.WriteLine(mensagem ?? $"Falha ao conectar no banco: {ex.Message}");
    return 1;
}

var menu = new MenuPrincipal(connectionFactory);
await menu.ExecutarAsync();

return 0;
