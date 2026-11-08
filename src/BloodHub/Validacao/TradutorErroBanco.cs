using MySqlConnector;

namespace BloodHub.Validacao;

public static class TradutorErroBanco
{
    public static string? Traduzir(MySqlException ex)
    {
        if (ex.Number == 1062)
        {
            if (ex.Message.Contains("cpf", StringComparison.OrdinalIgnoreCase))
                return "Já existe um doador cadastrado com este CPF.";

            return "Registro duplicado. Já existe um registro com estes dados.";
        }

        if (ex.Number == 1451)
        {
            if (ex.Message.Contains("fk_doacoes_doador", StringComparison.OrdinalIgnoreCase))
                return "Não é possível excluir este doador pois ele possui doações registradas.";

            return "Não é possível excluir este registro pois ele está sendo referenciado por outros dados.";
        }

        if (ex.Number == 1452)
        {
            return "Referência inválida. O registro relacionado não foi encontrado.";
        }

        if (ex.Number == 1406)
        {
            return "Um dos campos excede o tamanho máximo permitido.";
        }

        return null;
    }

    public static bool EhRestricaoIntegridade(MySqlException ex) => ex.Number == 1451;
}
