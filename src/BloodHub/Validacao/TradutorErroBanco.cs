using MySqlConnector;

namespace BloodHub.Validacao;

public static class TradutorErroBanco
{
    /// <summary>
    /// Traduz uma MySqlException em uma mensagem amigável para o usuário.
    /// Retorna null se a exceção não for reconhecida.
    /// </summary>
    public static string? Traduzir(MySqlException ex)
    {
        // Duplicate entry (UNIQUE constraint violation)
        if (ex.Number == 1062)
        {
            if (ex.Message.Contains("cpf", StringComparison.OrdinalIgnoreCase))
                return "Já existe um doador cadastrado com este CPF.";

            return "Registro duplicado. Já existe um registro com estes dados.";
        }

        // Foreign key constraint failure (cannot delete/update parent row)
        if (ex.Number == 1451)
        {
            if (ex.Message.Contains("fk_doacoes_doador", StringComparison.OrdinalIgnoreCase))
                return "Não é possível excluir este doador pois ele possui doações registradas.";

            return "Não é possível excluir este registro pois ele está sendo referenciado por outros dados.";
        }

        // Foreign key constraint failure (cannot add/update child row)
        if (ex.Number == 1452)
        {
            return "Referência inválida. O registro relacionado não foi encontrado.";
        }

        // Data too long for column
        if (ex.Number == 1406)
        {
            return "Um dos campos excede o tamanho máximo permitido.";
        }

        return null;
    }

    /// <summary>
    /// Verifica se a exceção é uma violação de chave estrangeira ao tentar excluir.
    /// </summary>
    public static bool EhRestricaoIntegridade(MySqlException ex) => ex.Number == 1451;
}
