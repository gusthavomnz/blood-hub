using System.ComponentModel.DataAnnotations;
using BloodHub.Data;
using BloodHub.Modelos;
using MySqlConnector;

namespace BloodHub.Repositorios;

public sealed class DoacaoRepositorio(ConnectionFactory factory) : IDoacaoRepositorio
{
    private const string ColunasSimples = @"
        id, doador_id, data_doacao, peso_kg, volume_ml, status, motivo_recusa, local_coleta, observacao, criado_em";

    private const string ColunasComDoador = @"
        d.id, d.doador_id, d.data_doacao, d.peso_kg, d.volume_ml, d.status, d.motivo_recusa, d.local_coleta, d.observacao, d.criado_em,
        doad.nome, doad.cpf, doad.data_nascimento, doad.tipo_sanguineo, doad.fator_rh, doad.sexo, doad.peso, doad.telefone, doad.email, doad.endereco, doad.ativo,
        doad.criado_em AS doad_criado_em, doad.atualizado_em AS doad_atualizado_em";

    public async Task<int> InserirAsync(Doacao doacao)
    {
        ArgumentNullException.ThrowIfNull(doacao);
        ValidarId(doacao.DoadorId, "O ID do doador informado é inválido.");

        const string sql = """
            INSERT INTO doacoes
                (doador_id, data_doacao, peso_kg, volume_ml, status, motivo_recusa, local_coleta, observacao)
            VALUES
                (@doadorId, @dataDoacao, @pesoKg, @volumeMl, @status, @motivoRecusa, @localColeta, @observacao);
            """;

        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        AdicionarParametros(comando, doacao);

        await comando.ExecuteNonQueryAsync();
        doacao.Id = checked((int)comando.LastInsertedId);
        return doacao.Id;
    }

    public async Task<bool> AtualizarAsync(Doacao doacao)
    {
        ArgumentNullException.ThrowIfNull(doacao);
        ValidarId(doacao.Id);
        ValidarId(doacao.DoadorId, "O ID do doador informado é inválido.");

        const string sql = """
            UPDATE doacoes
            SET doador_id = @doadorId,
                data_doacao = @dataDoacao,
                peso_kg = @pesoKg,
                volume_ml = @volumeMl,
                status = @status,
                motivo_recusa = @motivoRecusa,
                local_coleta = @localColeta,
                observacao = @observacao
            WHERE id = @id;
            """;

        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        AdicionarParametros(comando, doacao);
        comando.Parameters.AddWithValue("@id", doacao.Id);

        return await comando.ExecuteNonQueryAsync() > 0;
    }

    public Task<bool> ExcluirAsync(int id)
    {
        ValidarId(id);
        return ExecutarComandoAsync("DELETE FROM doacoes WHERE id = @id;", id);
    }

    public async Task<Doacao?> BuscarPorIdAsync(int id)
    {
        ValidarId(id);
        var doacoes = await ConsultarComDoadorAsync(
            $"""
            SELECT {ColunasComDoador}
            FROM doacoes d
            INNER JOIN doadores doad ON d.doador_id = doad.id
            WHERE d.id = @id;
            """,
            comando => comando.Parameters.AddWithValue("@id", id));

        return doacoes.SingleOrDefault();
    }

    public Task<List<Doacao>> ListarComDoadorAsync() => ConsultarComDoadorAsync(
        $"""
        SELECT {ColunasComDoador}
        FROM doacoes d
        INNER JOIN doadores doad ON d.doador_id = doad.id
        ORDER BY d.data_doacao DESC, d.id DESC;
        """);

    public Task<List<Doacao>> ListarPorDoadorAsync(int doadorId)
    {
        ValidarId(doadorId, "O ID do doador informado é inválido.");
        return ConsultarSimplesAsync(
            $"""
            SELECT {ColunasSimples}
            FROM doacoes
            WHERE doador_id = @doadorId
            ORDER BY data_doacao DESC, id DESC;
            """,
            comando => comando.Parameters.AddWithValue("@doadorId", doadorId));
    }

    public Task<List<Doacao>> ListarRealizadasDoDoadorAsync(int doadorId)
    {
        ValidarId(doadorId, "O ID do doador informado é inválido.");
        return ConsultarSimplesAsync(
            $"""
            SELECT {ColunasSimples}
            FROM doacoes
            WHERE doador_id = @doadorId AND status = 'Realizada'
            ORDER BY data_doacao DESC, id DESC;
            """,
            comando => comando.Parameters.AddWithValue("@doadorId", doadorId));
    }

    public Task<List<Doacao>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        if (inicio.Date > fim.Date)
            throw new ValidationException("A data inicial não pode ser superior à data final.");

        return ConsultarComDoadorAsync(
            $"""
            SELECT {ColunasComDoador}
            FROM doacoes d
            INNER JOIN doadores doad ON d.doador_id = doad.id
            WHERE d.data_doacao BETWEEN @inicio AND @fim
            ORDER BY d.data_doacao DESC, d.id DESC;
            """,
            comando =>
            {
                comando.Parameters.AddWithValue("@inicio", inicio.Date);
                comando.Parameters.AddWithValue("@fim", fim.Date);
            });
    }

    private async Task<List<Doacao>> ConsultarSimplesAsync(string sql, Action<MySqlCommand>? configurar = null)
    {
        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        configurar?.Invoke(comando);

        await using var leitor = await comando.ExecuteReaderAsync();
        var lista = new List<Doacao>();
        while (await leitor.ReadAsync())
            lista.Add(MapearSimples(leitor));

        return lista;
    }

    private async Task<List<Doacao>> ConsultarComDoadorAsync(string sql, Action<MySqlCommand>? configurar = null)
    {
        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        configurar?.Invoke(comando);

        await using var leitor = await comando.ExecuteReaderAsync();
        var lista = new List<Doacao>();
        while (await leitor.ReadAsync())
            lista.Add(MapearComDoador(leitor));

        return lista;
    }

    private async Task<bool> ExecutarComandoAsync(string sql, int id)
    {
        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@id", id);
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    private static void AdicionarParametros(MySqlCommand comando, Doacao doacao)
    {
        comando.Parameters.AddWithValue("@doadorId", doacao.DoadorId);
        comando.Parameters.AddWithValue("@dataDoacao", doacao.DataDoacao.Date);
        comando.Parameters.AddWithValue("@pesoKg", decimal.Round(doacao.PesoKg, 2));
        comando.Parameters.AddWithValue("@volumeMl", doacao.VolumeMl);
        comando.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(doacao.Status) ? "Realizada" : doacao.Status.Trim());
        comando.Parameters.AddWithValue("@motivoRecusa", string.IsNullOrWhiteSpace(doacao.MotivoRecusa) ? DBNull.Value : doacao.MotivoRecusa.Trim());
        comando.Parameters.AddWithValue("@localColeta", string.IsNullOrWhiteSpace(doacao.LocalColeta) ? "Hemocentro Central" : doacao.LocalColeta.Trim());
        comando.Parameters.AddWithValue("@observacao", string.IsNullOrWhiteSpace(doacao.Observacao) ? DBNull.Value : doacao.Observacao.Trim());
    }

    private static Doacao MapearSimples(MySqlDataReader leitor)
    {
        return new Doacao
        {
            Id = leitor.GetInt32("id"),
            DoadorId = leitor.GetInt32("doador_id"),
            DataDoacao = leitor.GetDateTime("data_doacao"),
            PesoKg = leitor.GetDecimal("peso_kg"),
            VolumeMl = leitor.GetInt32("volume_ml"),
            Status = leitor.GetString("status"),
            MotivoRecusa = leitor.IsDBNull(leitor.GetOrdinal("motivo_recusa")) ? null : leitor.GetString("motivo_recusa"),
            LocalColeta = leitor.GetString("local_coleta"),
            Observacao = leitor.IsDBNull(leitor.GetOrdinal("observacao")) ? null : leitor.GetString("observacao"),
            CriadoEm = leitor.GetDateTime("criado_em")
        };
    }

    private static Doacao MapearComDoador(MySqlDataReader leitor)
    {
        var doacao = MapearSimples(leitor);
        doacao.Doador = new Doador
        {
            Id = leitor.GetInt32("doador_id"),
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
            CriadoEm = leitor.GetDateTime("doad_criado_em"),
            AtualizadoEm = leitor.GetDateTime("doad_atualizado_em")
        };
        return doacao;
    }

    private static void ValidarId(int id, string mensagem = "O identificador deve ser um número inteiro positivo.")
    {
        if (id <= 0)
            throw new ValidationException(mensagem);
    }
}
