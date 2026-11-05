using System.ComponentModel.DataAnnotations;
using BloodHub.Data;
using BloodHub.Models;
using MySqlConnector;

namespace BloodHub.Checks;

public static class RepositorioChecks
{
    public static async Task<int> ExecutarAsync()
    {
        var nomeBanco = Environment.GetEnvironmentVariable("BLOODHUB_TEST_DATABASE");
        if (string.IsNullOrWhiteSpace(nomeBanco) ||
            !nomeBanco.EndsWith("_tests", StringComparison.Ordinal) ||
            nomeBanco.Length > 64 ||
            nomeBanco.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_'))
        {
            Console.Error.WriteLine("Integração recusada: defina BLOODHUB_TEST_DATABASE com um banco isolado cujo nome termine em _tests.");
            return 1;
        }

        var configuracao = DatabaseSettings.FromEnvironment() with { Database = nomeBanco };
        var conexoes = new ConnectionFactory(configuracao);
        var repositorio = new DoadorRepositorio(conexoes);
        var criados = new List<int>();
        var total = 0;
        var resultado = 0;

        void Verificar(bool condicao, string descricao)
        {
            if (!condicao)
                throw new InvalidOperationException(descricao);
            total++;
        }

        try
        {
            if ((await repositorio.ListarAsync()).Count != 0)
                throw new InvalidOperationException("O banco de testes precisa estar sem doadores. Nenhum registro existente foi alterado.");

            const string cpfA = "00123456797";
            var cpfB = GerarCpf("123456789");
            var doadorA = CriarDoador("  Doador Sintético %_! A  ", "001.234.567-97");
            doadorA.Sexo = " m ";
            doadorA.TipoSanguineo = " o+ ";
            doadorA.Telefone = "  ";
            doadorA.DataNascimento = new DateTime(1995, 5, 20, 14, 30, 0);

            var idA = await repositorio.InserirAsync(doadorA);
            criados.Add(idA);
            var idB = await repositorio.InserirAsync(CriarDoador("Doador Sintético B", cpfB));
            criados.Add(idB);
            Verificar(idA > 0 && idB > idA, "O cadastro deveria devolver IDs gerados pelo banco.");

            var salvoA = await repositorio.BuscarPorIdAsync(idA)
                ?? throw new InvalidOperationException("O doador cadastrado não foi encontrado pelo ID.");
            Verificar(salvoA.Cpf == cpfA, "O CPF deveria preservar zeros iniciais e perder somente a máscara.");
            Verificar(salvoA.Nome == "Doador Sintético %_! A", "O nome deveria ser normalizado e preservar acentos.");
            Verificar(salvoA.Sexo == "M" && salvoA.TipoSanguineo == "O+", "Sexo e tipo sanguíneo deveriam ser normalizados.");
            Verificar(salvoA.Telefone is null, "Telefone vazio deveria ser persistido como NULL.");
            Verificar(salvoA.DataNascimento == new DateTime(1995, 5, 20), "Nascimento deveria ser persistido como data.");
            Verificar(salvoA.Ativo, "Um novo cadastro deveria estar ativo.");
            Verificar((await repositorio.BuscarPorCpfAsync("001.234.567-97"))?.Id == idA, "A busca por CPF deveria aceitar máscara.");
            Verificar((await repositorio.ListarAsync()).Count == 2, "A listagem deveria incluir os dois registros.");
            Verificar((await repositorio.BuscarPorNomeAsync("Sintético")).Count == 2, "A busca parcial deveria encontrar ambos os nomes.");
            Verificar((await repositorio.BuscarPorNomeAsync("%")).SingleOrDefault()?.Id == idA, "O caractere % deveria ser pesquisado literalmente.");
            Verificar((await repositorio.BuscarPorNomeAsync("_")).SingleOrDefault()?.Id == idA, "O caractere _ deveria ser pesquisado literalmente.");
            Verificar((await repositorio.BuscarPorNomeAsync("!")).SingleOrDefault()?.Id == idA, "O caractere de escape deveria ser pesquisado literalmente.");
            Verificar((await repositorio.BuscarPorNomeAsync("' OR 1=1 --")).Count == 0, "Texto de injeção SQL não deveria retornar os doadores.");
            Verificar((await repositorio.BuscarPorNomeAsync("Nome que não existe")).Count == 0, "Uma busca sem resultados deveria retornar lista vazia.");
            Verificar(await repositorio.BuscarPorIdAsync(int.MaxValue) is null, "Um ID inexistente deveria retornar null.");
            Verificar(await repositorio.BuscarPorCpfAsync(GerarCpf("987654321")) is null, "Um CPF válido inexistente deveria retornar null.");

            Verificar(await repositorio.AtualizarAsync(salvoA), "Atualizar sem alterações deveria reconhecer o cadastro existente.");
            salvoA.Nome = "  Doador Atualizado %_! A  ";
            salvoA.Cpf = "001.234.567-97";
            salvoA.Telefone = "  85999999999  ";
            Verificar(await repositorio.AtualizarAsync(salvoA), "Atualizar mantendo o próprio CPF deveria funcionar.");
            var atualizadoA = await repositorio.BuscarPorIdAsync(idA)
                ?? throw new InvalidOperationException("O doador desapareceu após a atualização.");
            Verificar(atualizadoA.Nome == "Doador Atualizado %_! A" && atualizadoA.Cpf == cpfA,
                "A atualização deveria persistir nome normalizado e manter o CPF.");
            Verificar(atualizadoA.Telefone == "85999999999", "O telefone deveria ser normalizado na atualização.");

            var duplicadoInsercao = await EsperarErroAsync<ErroBancoException>(
                () => repositorio.InserirAsync(CriarDoador("Duplicado não deve persistir", cpfA)));
            Verificar(duplicadoInsercao.Message.Contains("CPF", StringComparison.OrdinalIgnoreCase),
                "A duplicidade no cadastro deveria produzir uma mensagem sobre CPF.");
            Verificar((await repositorio.ListarAsync()).Count == 2, "A tentativa de CPF duplicado não deveria criar um registro.");

            var salvoB = await repositorio.BuscarPorIdAsync(idB)
                ?? throw new InvalidOperationException("O segundo doador não foi encontrado.");
            salvoB.Cpf = cpfA;
            salvoB.Nome = "Alteração duplicada não deve persistir";
            var duplicadoAtualizacao = await EsperarErroAsync<ErroBancoException>(
                () => repositorio.AtualizarAsync(salvoB));
            Verificar(duplicadoAtualizacao.Message.Contains("CPF", StringComparison.OrdinalIgnoreCase),
                "A duplicidade na atualização deveria produzir uma mensagem sobre CPF.");
            var intactoB = await repositorio.BuscarPorIdAsync(idB);
            Verificar(intactoB?.Cpf == cpfB && intactoB.Nome == "Doador Sintético B",
                "Uma atualização rejeitada deveria preservar os dados anteriores.");

            await EsperarErroAsync<ValidationException>(
                () => repositorio.InserirAsync(CriarDoador("CPF inválido não deve persistir", "11111111111")));
            Verificar((await repositorio.ListarAsync()).Count == 2, "Entrada inválida deveria ser rejeitada sem gravar dados.");

            Verificar(await repositorio.InativarAsync(idA), "A inativação deveria encontrar o doador.");
            Verificar(await repositorio.InativarAsync(idA), "Inativar novamente deveria continuar sendo uma operação válida.");
            var inativoA = await repositorio.BuscarPorIdAsync(idA);
            Verificar(inativoA is { Ativo: false } && inativoA.Cpf == cpfA,
                "Inativar deveria preservar o cadastro e alterar somente sua situação.");
            Verificar((await repositorio.ListarAsync()).Any(d => d.Id == idA && !d.Ativo),
                "A listagem deveria continuar mostrando o doador inativo.");

            await InserirDoacaoRecusadaAsync(conexoes, idA);
            var exclusaoBloqueada = await EsperarErroAsync<ErroBancoException>(() => repositorio.ExcluirAsync(idA));
            Verificar(exclusaoBloqueada.Message.Contains("doações", StringComparison.OrdinalIgnoreCase) &&
                      exclusaoBloqueada.Message.Contains("Inative", StringComparison.OrdinalIgnoreCase),
                "A exclusão bloqueada deveria explicar o vínculo e oferecer inativação.");
            Verificar(await repositorio.BuscarPorIdAsync(idA) is not null,
                "O doador deveria permanecer após a tentativa de exclusão bloqueada.");
            Verificar(await ContarDoacoesAsync(conexoes, idA) == 1,
                "Até uma doação recusada deveria bloquear a exclusão e permanecer no histórico.");

            Verificar(await repositorio.ExcluirAsync(idB), "Um doador sem doações deveria poder ser excluído.");
            Verificar(await repositorio.BuscarPorIdAsync(idB) is null, "O doador excluído não deveria ser encontrado.");
            Verificar(!await repositorio.ExcluirAsync(idB), "Excluir um ID ausente deveria retornar false.");
            Verificar(!await repositorio.InativarAsync(idB), "Inativar um ID ausente deveria retornar false.");
            intactoB!.Id = idB;
            Verificar(!await repositorio.AtualizarAsync(intactoB), "Atualizar um ID ausente deveria retornar false.");

            Console.WriteLine($"PASSOU: integração MySQL — {total} verificações em {nomeBanco}.");
        }
        catch (Exception erro)
        {
            Console.Error.WriteLine($"FALHOU: integração MySQL: {erro.Message}");
            resultado = 1;
        }
        finally
        {
            if (criados.Count > 0)
            {
                try
                {
                    await LimparRegistrosCriadosAsync(conexoes, criados);
                }
                catch (Exception erro)
                {
                    Console.Error.WriteLine($"FALHOU: limpeza dos registros de teste: {erro.Message}");
                    resultado = 1;
                }
            }
        }

        return resultado;
    }

    private static Doador CriarDoador(string nome, string cpf) => new()
    {
        Nome = nome,
        Cpf = cpf,
        DataNascimento = new DateTime(1995, 5, 20),
        Sexo = "F",
        TipoSanguineo = "AB-",
        Ativo = true
    };

    private static string GerarCpf(string noveDigitos)
    {
        var cpf = noveDigitos;
        for (var tamanho = 9; tamanho <= 10; tamanho++)
        {
            var soma = 0;
            for (var indice = 0; indice < tamanho; indice++)
                soma += (cpf[indice] - '0') * (tamanho + 1 - indice);
            var resto = soma % 11;
            cpf += resto < 2 ? '0' : (char)('0' + 11 - resto);
        }
        return cpf;
    }

    private static async Task<T> EsperarErroAsync<T>(Func<Task> acao) where T : Exception
    {
        try
        {
            await acao();
        }
        catch (T erro)
        {
            return erro;
        }
        throw new InvalidOperationException($"Era esperado um erro {typeof(T).Name}.");
    }

    private static async Task InserirDoacaoRecusadaAsync(ConnectionFactory conexoes, int doadorId)
    {
        const string sql = """
            INSERT INTO doacao (doador_id, data_doacao, peso_kg, volume_ml, status, motivo_recusa, local_coleta)
            VALUES (@doador, @data, @peso, @volume, @status, @motivo, @local);
            """;
        await using var conexao = await conexoes.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@doador", doadorId);
        comando.Parameters.AddWithValue("@data", new DateTime(2026, 1, 15, 9, 0, 0));
        comando.Parameters.AddWithValue("@peso", 60m);
        comando.Parameters.AddWithValue("@volume", 0);
        comando.Parameters.AddWithValue("@status", "Recusada");
        comando.Parameters.AddWithValue("@motivo", "Registro sintético para verificar integridade referencial.");
        comando.Parameters.AddWithValue("@local", "Unidade de testes");
        await comando.ExecuteNonQueryAsync();
    }

    private static async Task<long> ContarDoacoesAsync(ConnectionFactory conexoes, int doadorId)
    {
        await using var conexao = await conexoes.OpenAsync();
        await using var comando = new MySqlCommand("SELECT COUNT(*) FROM doacao WHERE doador_id = @id;", conexao);
        comando.Parameters.AddWithValue("@id", doadorId);
        return Convert.ToInt64(await comando.ExecuteScalarAsync());
    }

    private static async Task LimparRegistrosCriadosAsync(ConnectionFactory conexoes, List<int> ids)
    {
        await using var conexao = await conexoes.OpenAsync();
        foreach (var id in ids)
        {
            await using var doacoes = new MySqlCommand("DELETE FROM doacao WHERE doador_id = @id;", conexao);
            doacoes.Parameters.AddWithValue("@id", id);
            await doacoes.ExecuteNonQueryAsync();

            await using var doador = new MySqlCommand("DELETE FROM doador WHERE id = @id;", conexao);
            doador.Parameters.AddWithValue("@id", id);
            await doador.ExecuteNonQueryAsync();
        }
    }
}
