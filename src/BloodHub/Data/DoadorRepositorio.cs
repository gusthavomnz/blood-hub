using System.ComponentModel.DataAnnotations;
using BloodHub.Models;
using BloodHub.Regras;
using MySqlConnector;

namespace BloodHub.Data;

public sealed class DoadorRepositorio(ConnectionFactory connectionFactory)
{
    private const string Colunas =
        "id, nome, cpf, data_nascimento, sexo, tipo_sanguineo, telefone, ativo";

    public async Task<int> InserirAsync(Doador doador)
    {
        var dados = Validadores.NormalizarEValidar(doador);
        const string sql = """
            INSERT INTO doador (nome, cpf, data_nascimento, sexo, tipo_sanguineo, telefone, ativo)
            VALUES (@nome, @cpf, @nascimento, @sexo, @tipo, @telefone, @ativo);
            """;

        try
        {
            await using var conexao = await connectionFactory.OpenAsync();
            await using var comando = new MySqlCommand(sql, conexao);
            AdicionarDados(comando, dados);
            await comando.ExecuteNonQueryAsync();
            return checked((int)comando.LastInsertedId);
        }
        catch (MySqlException erro)
        {
            throw TradutorErroBanco.Traduzir(erro, "cadastrar o doador");
        }
    }

    public Task<List<Doador>> ListarAsync() =>
        ConsultarAsync($"SELECT {Colunas} FROM doador ORDER BY nome, id;");

    public async Task<Doador?> BuscarPorIdAsync(int id)
    {
        ValidarId(id);
        var doadores = await ConsultarAsync(
            $"SELECT {Colunas} FROM doador WHERE id = @id;",
            comando => comando.Parameters.AddWithValue("@id", id));
        return doadores.SingleOrDefault();
    }

    public async Task<Doador?> BuscarPorCpfAsync(string cpf)
    {
        var normalizado = Validadores.NormalizarCpf(cpf);
        var doadores = await ConsultarAsync(
            $"SELECT {Colunas} FROM doador WHERE cpf = @cpf;",
            comando => comando.Parameters.AddWithValue("@cpf", normalizado));
        return doadores.SingleOrDefault();
    }

    public Task<List<Doador>> BuscarPorNomeAsync(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidationException("Informe um nome ou parte dele para buscar.");

        var trecho = nome.Trim();
        if (trecho.Length > 120)
            throw new ValidationException("O nome para busca deve ter no máximo 120 caracteres.");

        // O usuário pesquisa texto literal, mesmo quando digita os curingas do LIKE.
        var padrao = "%" + trecho.Replace("!", "!!").Replace("%", "!%").Replace("_", "!_") + "%";
        return ConsultarAsync(
            $"SELECT {Colunas} FROM doador WHERE nome LIKE @nome ESCAPE '!' ORDER BY nome, id;",
            comando => comando.Parameters.AddWithValue("@nome", padrao));
    }

    public Task<bool> AtualizarAsync(Doador doador)
    {
        var dados = Validadores.NormalizarEValidar(doador);
        ValidarId(dados.Id);
        const string sql = """
            UPDATE doador
            SET nome = @nome, cpf = @cpf, data_nascimento = @nascimento,
                sexo = @sexo, tipo_sanguineo = @tipo, telefone = @telefone, ativo = @ativo
            WHERE id = @id;
            """;

        return ExecutarAlteracaoAsync(sql, comando =>
        {
            AdicionarDados(comando, dados);
            comando.Parameters.AddWithValue("@id", dados.Id);
        }, "atualizar o doador");
    }

    public Task<bool> InativarAsync(int id)
    {
        ValidarId(id);
        return ExecutarAlteracaoAsync(
            "UPDATE doador SET ativo = FALSE WHERE id = @id;",
            comando => comando.Parameters.AddWithValue("@id", id),
            "inativar o doador");
    }

    public Task<bool> ExcluirAsync(int id)
    {
        ValidarId(id);
        // A FK RESTRICT protege o histórico, inclusive contra alterações concorrentes.
        return ExecutarAlteracaoAsync(
            "DELETE FROM doador WHERE id = @id;",
            comando => comando.Parameters.AddWithValue("@id", id),
            "excluir o doador");
    }

    private async Task<List<Doador>> ConsultarAsync(string sql, Action<MySqlCommand>? configurar = null)
    {
        try
        {
            await using var conexao = await connectionFactory.OpenAsync();
            await using var comando = new MySqlCommand(sql, conexao);
            configurar?.Invoke(comando);
            await using var leitor = await comando.ExecuteReaderAsync();
            var doadores = new List<Doador>();
            while (await leitor.ReadAsync())
                doadores.Add(Mapear(leitor));
            return doadores;
        }
        catch (MySqlException erro)
        {
            throw TradutorErroBanco.Traduzir(erro, "consultar os doadores");
        }
    }

    private async Task<bool> ExecutarAlteracaoAsync(string sql, Action<MySqlCommand> configurar, string operacao)
    {
        try
        {
            await using var conexao = await connectionFactory.OpenAsync();
            await using var comando = new MySqlCommand(sql, conexao);
            configurar(comando);
            return await comando.ExecuteNonQueryAsync() > 0;
        }
        catch (MySqlException erro)
        {
            throw TradutorErroBanco.Traduzir(erro, operacao);
        }
    }

    private static void AdicionarDados(MySqlCommand comando, Doador doador)
    {
        comando.Parameters.AddWithValue("@nome", doador.Nome);
        comando.Parameters.AddWithValue("@cpf", doador.Cpf);
        comando.Parameters.AddWithValue("@nascimento", doador.DataNascimento.Date);
        comando.Parameters.AddWithValue("@sexo", doador.Sexo);
        comando.Parameters.AddWithValue("@tipo", doador.TipoSanguineo);
        comando.Parameters.AddWithValue("@telefone", (object?)doador.Telefone ?? DBNull.Value);
        comando.Parameters.AddWithValue("@ativo", doador.Ativo);
    }

    private static Doador Mapear(MySqlDataReader leitor) => new()
    {
        Id = leitor.GetInt32("id"),
        Nome = leitor.GetString("nome"),
        Cpf = leitor.GetString("cpf"),
        DataNascimento = leitor.GetDateTime("data_nascimento"),
        Sexo = leitor.GetString("sexo"),
        TipoSanguineo = leitor.GetString("tipo_sanguineo"),
        Telefone = leitor.IsDBNull(leitor.GetOrdinal("telefone")) ? null : leitor.GetString("telefone"),
        Ativo = leitor.GetBoolean("ativo")
    };

    private static void ValidarId(int id)
    {
        if (id <= 0)
            throw new ValidationException("O ID do doador deve ser um número positivo.");
    }
}
