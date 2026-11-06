using BloodHub.Data;
using BloodHub.Modelos;
using MySqlConnector;

namespace BloodHub.Repositorios;

public class DoadorRepositorio
{
    private readonly ConnectionFactory _factory;

    public DoadorRepositorio(ConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task InserirAsync(Doador doador)
    {
        await using var conn = await _factory.OpenAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO doadores (nome, cpf, data_nascimento, tipo_sanguineo, fator_rh, sexo, peso, telefone, email, endereco, ativo)
            VALUES (@nome, @cpf, @dataNascimento, @tipoSanguineo, @fatorRh, @sexo, @peso, @telefone, @email, @endereco, @ativo)";

        cmd.Parameters.AddWithValue("@nome", doador.Nome);
        cmd.Parameters.AddWithValue("@cpf", doador.Cpf);
        cmd.Parameters.AddWithValue("@dataNascimento", doador.DataNascimento);
        cmd.Parameters.AddWithValue("@tipoSanguineo", doador.TipoSanguineo);
        cmd.Parameters.AddWithValue("@fatorRh", doador.FatorRh);
        cmd.Parameters.AddWithValue("@sexo", doador.Sexo);
        cmd.Parameters.AddWithValue("@peso", doador.Peso);
        cmd.Parameters.AddWithValue("@telefone", doador.Telefone);
        cmd.Parameters.AddWithValue("@email", (object?)doador.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@endereco", (object?)doador.Endereco ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ativo", doador.Ativo);

        await cmd.ExecuteNonQueryAsync();
        doador.Id = (int)cmd.LastInsertedId;
    }

    public async Task<List<Doador>> ListarTodosAsync()
    {
        await using var conn = await _factory.OpenAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT * FROM doadores ORDER BY nome";

        await using var reader = await cmd.ExecuteReaderAsync();
        return await LerListaAsync(reader);
    }

    public async Task<Doador?> BuscarPorIdAsync(int id)
    {
        await using var conn = await _factory.OpenAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT * FROM doadores WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        var lista = await LerListaAsync(reader);
        return lista.Count > 0 ? lista[0] : null;
    }

    public async Task<Doador?> BuscarPorCpfAsync(string cpf)
    {
        await using var conn = await _factory.OpenAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT * FROM doadores WHERE cpf = @cpf";
        cmd.Parameters.AddWithValue("@cpf", cpf);

        await using var reader = await cmd.ExecuteReaderAsync();
        var lista = await LerListaAsync(reader);
        return lista.Count > 0 ? lista[0] : null;
    }

    public async Task<List<Doador>> BuscarPorNomeAsync(string nome)
    {
        await using var conn = await _factory.OpenAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT * FROM doadores WHERE nome LIKE @nome ORDER BY nome";
        cmd.Parameters.AddWithValue("@nome", $"%{nome}%");

        await using var reader = await cmd.ExecuteReaderAsync();
        return await LerListaAsync(reader);
    }

    public async Task AtualizarAsync(Doador doador)
    {
        await using var conn = await _factory.OpenAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
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
            WHERE id = @id";

        cmd.Parameters.AddWithValue("@id", doador.Id);
        cmd.Parameters.AddWithValue("@nome", doador.Nome);
        cmd.Parameters.AddWithValue("@cpf", doador.Cpf);
        cmd.Parameters.AddWithValue("@dataNascimento", doador.DataNascimento);
        cmd.Parameters.AddWithValue("@tipoSanguineo", doador.TipoSanguineo);
        cmd.Parameters.AddWithValue("@fatorRh", doador.FatorRh);
        cmd.Parameters.AddWithValue("@sexo", doador.Sexo);
        cmd.Parameters.AddWithValue("@peso", doador.Peso);
        cmd.Parameters.AddWithValue("@telefone", doador.Telefone);
        cmd.Parameters.AddWithValue("@email", (object?)doador.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@endereco", (object?)doador.Endereco ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ativo", doador.Ativo);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        await using var conn = await _factory.OpenAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = "DELETE FROM doadores WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task InativarAsync(int id)
    {
        await using var conn = await _factory.OpenAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = "UPDATE doadores SET ativo = 0 WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<List<Doador>> LerListaAsync(MySqlDataReader reader)
    {
        var lista = new List<Doador>();

        while (await reader.ReadAsync())
        {
            lista.Add(new Doador
            {
                Id = reader.GetInt32("id"),
                Nome = reader.GetString("nome"),
                Cpf = reader.GetString("cpf"),
                DataNascimento = reader.GetDateTime("data_nascimento"),
                TipoSanguineo = reader.GetString("tipo_sanguineo"),
                FatorRh = reader.GetString("fator_rh"),
                Sexo = reader.GetString("sexo"),
                Peso = reader.GetDecimal("peso"),
                Telefone = reader.GetString("telefone"),
                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email"),
                Endereco = reader.IsDBNull(reader.GetOrdinal("endereco")) ? null : reader.GetString("endereco"),
                Ativo = reader.GetBoolean("ativo"),
                CriadoEm = reader.GetDateTime("criado_em"),
                AtualizadoEm = reader.GetDateTime("atualizado_em"),
            });
        }

        return lista;
    }
}
