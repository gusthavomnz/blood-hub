using MySqlConnector;

namespace BloodHub.Data;

public static class TradutorErroBanco
{
    public static ErroBancoException Traduzir(MySqlException erro, string operacao)
    {
        var mensagem = erro.Number switch
        {
            1062 => "Já existe um doador cadastrado com esse CPF.",
            1451 => "Este doador possui doações vinculadas e não pode ser excluído. Inative o cadastro para preservar o histórico.",
            1045 => "Acesso ao banco negado. Confira o usuário e a senha configurados.",
            1049 => "O banco configurado não existe. Confira a configuração do projeto.",
            1146 => "As tabelas do projeto ainda não foram criadas. Aplique a migration 002 conforme o README.",
            0 or 1042 or 2002 or 2003 => "Não foi possível conectar ao banco. Verifique se o MySQL está em execução.",
            _ => $"Não foi possível {operacao}. O banco de dados retornou uma falha."
        };

        return new ErroBancoException(mensagem, erro);
    }
}
