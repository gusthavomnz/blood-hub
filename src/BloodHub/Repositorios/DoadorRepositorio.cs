using System.ComponentModel.DataAnnotations;
using BloodHub.Data;
using BloodHub.Modelos;
using BloodHub.Validacao;
using MySqlConnector;

namespace BloodHub.Repositorios;

public sealed class DoadorRepositorio(ConnectionFactory factory)
{
    private const string Colunas = "id, nome, cpf, data_nascimento, tipo_sanguineo, fator_rh, sexo, peso, telefone, email, endereco, ativo, criado_em, atualizado_em";

    public async Task<int> InserirAsync(Doador doador)
    {
        var dados = Validadores.NormalizarEValidar(doador);
        const string sql = """
            INSERT INTO doadores
                (nome, cpf, data_nascimento, tipo_sanguineo, fator_rh, sexo, peso, telefone, email, endereco, ativo)
            VALUES
                (@nome, @cpf, @dataNascimento, @tipoSanguineo, @fatorRh, @sexo, @peso, @telefone, @email, @endereco, @ativo);
            """;

        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        AdicionarParametros(comando, dados);
        await comando.ExecuteNonQueryAsync();
        doador.Id = checked((int)comando.LastInsertedId);
        return doador.Id;
    }

    public Task<List<Doador>> ListarTodosAsync() => ConsultarAsync(
        $"SELECT {Colunas} FROM doadores ORDER BY nome, id;");

    public async Task<Doador?> BuscarPorIdAsync(int id)
    {
        ValidarId(id);
        var doadores = await ConsultarAsync(
            $"SELECT {Colunas} FROM doadores WHERE id = @id;",
            comando => comando.Parameters.AddWithValue("@id", id));
        return doadores.SingleOrDefault();
    }

    public async Task<Doador?> BuscarPorCpfAsync(string cpf)
    {
        var cpfNormalizado = Validadores.NormalizarCpf(cpf);
        var doadores = await ConsultarAsync(
            $"SELECT {Colunas} FROM doadores WHERE cpf = @cpf;",
            comando => comando.Parameters.AddWithValue("@cpf", cpfNormalizado));
        return doadores.SingleOrDefault();
    }

    public Task<List<Doador>> BuscarPorNomeAsync(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidationException("Informe um nome ou parte dele para buscar.");

        var trecho = nome.Trim();
        if (trecho.Length > 120)
            throw new ValidationException("O nome para busca deve ter no máximo 120 caracteres.");

        var padrao = "%" + trecho.Replace("!", "!!").Replace("%", "!%").Replace("_", "!_") + "%";
        return ConsultarAsync(
            $"SELECT {Colunas} FROM doadores WHERE nome LIKE @nome ESCAPE '!' ORDER BY nome, id;",
            comando => comando.Parameters.AddWithValue("@nome", padrao));
    }

    public async Task<bool> AtualizarAsync(Doador doador)
    {
        ValidarId(doador.Id);
        var dados = Validadores.NormalizarEValidar(doador);
        const string sql = """
            UPDATE doadores
            SET nome = @nome,
                cpf = @cpf,
                data_nascimento = @dataNascimento,
                tipo_sanguineo = @tipoSanguineo,
                fator_rh = @fatorRh,
                sexo = @sexo,
                peso = @peso,
                telefone = @telefone,
                email = @email,
                endereco = @endereco,
                ativo = @ativo
            WHERE id = @id;
            """;

        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        AdicionarParametros(comando, dados);
        comando.Parameters.AddWithValue("@id", dados.Id);
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    public Task<bool> ExcluirAsync(int id)
    {
        ValidarId(id);
        return ExecutarAsync("DELETE FROM doadores WHERE id = @id;", id);
    }

    public Task<bool> InativarAsync(int id)
    {
        ValidarId(id);
        return ExecutarAsync("UPDATE doadores SET ativo = FALSE WHERE id = @id;", id);
    }

    private async Task<List<Doador>> ConsultarAsync(string sql, Action<MySqlCommand>? configurar = null)
    {
        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        configurar?.Invoke(comando);
        await using var leitor = await comando.ExecuteReaderAsync();

        var doadores = new List<Doador>();
        while (await leitor.ReadAsync())
            doadores.Add(Mapear(leitor));
        return doadores;
    }

    private async Task<bool> ExecutarAsync(string sql, int id)
    {
        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@id", id);
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    private static void AdicionarParametros(MySqlCommand comando, Doador doador)
    {
        comando.Parameters.AddWithValue("@nome", doador.Nome);
        comando.Parameters.AddWithValue("@cpf", doador.Cpf);
        comando.Parameters.AddWithValue("@dataNascimento", doador.DataNascimento.Date);
        comando.Parameters.AddWithValue("@tipoSanguineo", doador.TipoSanguineo);
        comando.Parameters.AddWithValue("@fatorRh", doador.FatorRh);
        comando.Parameters.AddWithValue("@sexo", doador.Sexo);
        comando.Parameters.AddWithValue("@peso", doador.Peso);
        comando.Parameters.AddWithValue("@telefone", (object?)doador.Telefone ?? DBNull.Value);
        comando.Parameters.AddWithValue("@email", (object?)doador.Email ?? DBNull.Value);
        comando.Parameters.AddWithValue("@endereco", (object?)doador.Endereco ?? DBNull.Value);
        comando.Parameters.AddWithValue("@ativo", doador.Ativo);
    }

    private static Doador Mapear(MySqlDataReader leitor) => new()
    {
        Id = leitor.GetInt32("id"),
        Nome = leitor.GetString("nome"),
        Cpf = leitor.GetString("cpf"),
        DataNascimento = leitor.GetDateTime("data_nascimento"),
        TipoSanguineo = leitor.GetString("tipo_sanguineo"),
        FatorRh = leitor.GetString("fator_rh"),
        Sexo = leitor.GetString("sexo"),
        Peso = leitor.GetDecimal("peso"),
        Telefone = leitor.IsDBNull(leitor.GetOrdinal("telefone")) ? null : leitor.GetString("telefone"),
        Email = leitor.IsDBNull(leitor.GetOrdinal("email")) ? null : leitor.GetString("email"),
        Endereco = leitor.IsDBNull(leitor.GetOrdinal("endereco")) ? null : leitor.GetString("endereco"),
        Ativo = leitor.GetBoolean("ativo"),
        CriadoEm = leitor.GetDateTime("criado_em"),
        AtualizadoEm = leitor.GetDateTime("atualizado_em")
    };

    private static void ValidarId(int id)
    {
        if (id <= 0)
            throw new ValidationException("O ID do doador deve ser um número positivo.");
    }
}
