using System.ComponentModel.DataAnnotations;
using BloodHub.Modelos;
using BloodHub.Repositorios;
using BloodHub.Validacao;
using MySqlConnector;

namespace BloodHub.Telas;

/// <summary>
/// Interface de terminal para o CRUD de Doador.
/// Consome DoadorRepositorio, Validadores e TradutorErroBanco.
/// </summary>
public class TelaDoador
{
    private readonly DoadorRepositorio _repositorio;

    public TelaDoador(DoadorRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Exibe o submenu de doadores e processa a opção escolhida.
    /// </summary>
    public async Task ExibirMenuAsync()
    {
        while (true)
        {
            var opcao = LeitorConsole.LerOpcaoMenu("Gerenciar Doadores", [
                "Cadastrar doador",
                "Listar doadores",
                "Buscar doador",
                "Atualizar doador",
                "Excluir doador",
                "Voltar ao menu principal"
            ]);

            try
            {
                switch (opcao)
                {
                    case 1: await CadastrarAsync(); break;
                    case 2: await ListarAsync(); break;
                    case 3: await BuscarAsync(); break;
                    case 4: await AtualizarAsync(); break;
                    case 5: await ExcluirAsync(); break;
                    case 6: return;
                }
            }
            catch (MySqlException ex)
            {
                var msg = TradutorErroBanco.Traduzir(ex);
                LeitorConsole.ExibirErro(msg ?? $"Erro de banco de dados: {ex.Message}");
            }
            catch (ValidationException ex)
            {
                LeitorConsole.ExibirErro(ex.Message);
            }
            catch (Exception ex)
            {
                LeitorConsole.ExibirErro($"Erro inesperado: {ex.Message}");
            }

            LeitorConsole.AguardarEnter();
        }
    }

    // ──────────────────────── CADASTRAR ────────────────────────

    private async Task CadastrarAsync()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("══════ Cadastrar Novo Doador ══════");
        Console.ResetColor();

        var doador = LerDadosDoador();

        // Validar dados antes de salvar
        var erros = Validadores.ValidarDoador(doador);
        if (erros.Count > 0)
        {
            Console.WriteLine();
            LeitorConsole.ExibirErro("Erros de validação encontrados:");
            foreach (var erro in erros)
                LeitorConsole.ExibirErro($"  • {erro}");
            return;
        }

        // Formatar CPF para armazenamento
        doador.Cpf = Validadores.FormatarCpf(doador.Cpf);

        try
        {
            await _repositorio.InserirAsync(doador);
            LeitorConsole.ExibirSucesso($"Doador cadastrado com sucesso! ID: {doador.Id}");
        }
        catch (MySqlException ex)
        {
            var msg = TradutorErroBanco.Traduzir(ex);
            LeitorConsole.ExibirErro(msg ?? $"Erro ao cadastrar: {ex.Message}");
        }
    }

    // ──────────────────────── LISTAR ────────────────────────

    private async Task ListarAsync()
    {
        var doadores = await _repositorio.ListarTodosAsync();
        TelaRelatorios.ExibirTabelaDoadores(doadores);
    }

    // ──────────────────────── BUSCAR ────────────────────────

    private async Task BuscarAsync()
    {
        var opcao = LeitorConsole.LerOpcaoMenu("Buscar Doador por", [
            "ID",
            "CPF",
            "Nome",
            "Voltar"
        ]);

        switch (opcao)
        {
            case 1:
                await BuscarPorIdAsync();
                break;
            case 2:
                await BuscarPorCpfAsync();
                break;
            case 3:
                await BuscarPorNomeAsync();
                break;
            case 4:
                return;
        }
    }

    private async Task BuscarPorIdAsync()
    {
        var id = LeitorConsole.LerInt("ID do doador: ");
        var doador = await _repositorio.BuscarPorIdAsync(id);

        if (doador == null)
        {
            LeitorConsole.ExibirErro($"Doador com ID {id} não encontrado.");
            return;
        }

        TelaRelatorios.ExibirDetalhesDoador(doador);
    }

    private async Task BuscarPorCpfAsync()
    {
        var cpf = LeitorConsole.LerTexto("CPF do doador: ");
        cpf = Validadores.FormatarCpf(cpf);

        var doador = await _repositorio.BuscarPorCpfAsync(cpf);

        if (doador == null)
        {
            LeitorConsole.ExibirErro($"Doador com CPF {cpf} não encontrado.");
            return;
        }

        TelaRelatorios.ExibirDetalhesDoador(doador);
    }

    private async Task BuscarPorNomeAsync()
    {
        var nome = LeitorConsole.LerTexto("Nome (ou parte do nome): ");
        var doadores = await _repositorio.BuscarPorNomeAsync(nome);

        if (doadores.Count == 0)
        {
            LeitorConsole.ExibirErro($"Nenhum doador encontrado com o nome '{nome}'.");
            return;
        }

        TelaRelatorios.ExibirTabelaDoadores(doadores);
    }

    // ──────────────────────── ATUALIZAR ────────────────────────

    private async Task AtualizarAsync()
    {
        var id = LeitorConsole.LerInt("ID do doador a atualizar: ");
        var doador = await _repositorio.BuscarPorIdAsync(id);

        if (doador == null)
        {
            LeitorConsole.ExibirErro($"Doador com ID {id} não encontrado.");
            return;
        }

        TelaRelatorios.ExibirDetalhesDoador(doador);
        Console.WriteLine();
        LeitorConsole.ExibirInfo("Preencha os novos dados (deixe em branco para manter o valor atual).");
        Console.WriteLine();

        // Ler novos dados, mantendo os atuais se vazio
        var nome = LeitorConsole.LerTextoOpcional($"Nome [{doador.Nome}]: ");
        if (nome != null) doador.Nome = nome;

        var cpf = LeitorConsole.LerTextoOpcional($"CPF [{doador.Cpf}]: ");
        if (cpf != null) doador.Cpf = Validadores.FormatarCpf(cpf);

        var dataNascStr = LeitorConsole.LerTextoOpcional($"Data de nascimento [{doador.DataNascimento:dd/MM/yyyy}]: ");
        if (dataNascStr != null)
        {
            if (DateTime.TryParseExact(dataNascStr, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dataNasc))
            {
                doador.DataNascimento = dataNasc;
            }
            else
            {
                LeitorConsole.ExibirErro("Data inválida, mantendo o valor atual.");
            }
        }

        var tipo = LeitorConsole.LerTextoOpcional($"Tipo sanguíneo [{doador.TipoSanguineo}]: ");
        if (tipo != null) doador.TipoSanguineo = tipo.ToUpper();

        var fator = LeitorConsole.LerTextoOpcional($"Fator Rh [{doador.FatorRh}]: ");
        if (fator != null) doador.FatorRh = fator;

        var sexo = LeitorConsole.LerTextoOpcional($"Sexo [{doador.Sexo}]: ");
        if (sexo != null) doador.Sexo = sexo.ToUpper();

        var pesoStr = LeitorConsole.LerTextoOpcional($"Peso [{doador.Peso:F1}]: ");
        if (pesoStr != null)
        {
            if (decimal.TryParse(pesoStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.GetCultureInfo("pt-BR"), out var peso))
            {
                doador.Peso = peso;
            }
            else if (decimal.TryParse(pesoStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out peso))
            {
                doador.Peso = peso;
            }
            else
            {
                LeitorConsole.ExibirErro("Peso inválido, mantendo o valor atual.");
            }
        }

        var telefone = LeitorConsole.LerTextoOpcional($"Telefone [{doador.Telefone}]: ");
        if (telefone != null) doador.Telefone = telefone;

        var email = LeitorConsole.LerTextoOpcional($"E-mail [{doador.Email ?? ""}]: ");
        if (email != null) doador.Email = email;

        var endereco = LeitorConsole.LerTextoOpcional($"Endereço [{doador.Endereco ?? ""}]: ");
        if (endereco != null) doador.Endereco = endereco;

        // Validar dados atualizados
        var erros = Validadores.ValidarDoador(doador);
        if (erros.Count > 0)
        {
            Console.WriteLine();
            LeitorConsole.ExibirErro("Erros de validação encontrados:");
            foreach (var erro in erros)
                LeitorConsole.ExibirErro($"  • {erro}");
            return;
        }

        if (!LeitorConsole.LerConfirmacao("Confirma a atualização?"))
        {
            LeitorConsole.ExibirInfo("Atualização cancelada.");
            return;
        }

        try
        {
            await _repositorio.AtualizarAsync(doador);
            LeitorConsole.ExibirSucesso("Doador atualizado com sucesso!");
        }
        catch (MySqlException ex)
        {
            var msg = TradutorErroBanco.Traduzir(ex);
            LeitorConsole.ExibirErro(msg ?? $"Erro ao atualizar: {ex.Message}");
        }
    }

    // ──────────────────────── EXCLUIR / INATIVAR ────────────────────────

    private async Task ExcluirAsync()
    {
        var id = LeitorConsole.LerInt("ID do doador a excluir: ");
        var doador = await _repositorio.BuscarPorIdAsync(id);

        if (doador == null)
        {
            LeitorConsole.ExibirErro($"Doador com ID {id} não encontrado.");
            return;
        }

        TelaRelatorios.ExibirDetalhesDoador(doador);
        Console.WriteLine();

        if (!LeitorConsole.LerConfirmacao("Tem certeza que deseja EXCLUIR este doador?"))
        {
            LeitorConsole.ExibirInfo("Operação cancelada.");
            return;
        }

        try
        {
            await _repositorio.ExcluirAsync(id);
            LeitorConsole.ExibirSucesso("Doador excluído com sucesso!");
        }
        catch (MySqlException ex) when (TradutorErroBanco.EhRestricaoIntegridade(ex))
        {
            var msg = TradutorErroBanco.Traduzir(ex);
            LeitorConsole.ExibirErro(msg ?? "Não é possível excluir este doador.");
            Console.WriteLine();

            if (LeitorConsole.LerConfirmacao("Deseja INATIVAR o doador em vez de excluí-lo?"))
            {
                try
                {
                    await _repositorio.InativarAsync(id);
                    LeitorConsole.ExibirSucesso("Doador inativado com sucesso!");
                }
                catch (MySqlException exInativar)
                {
                    var msgInativar = TradutorErroBanco.Traduzir(exInativar);
                    LeitorConsole.ExibirErro(msgInativar ?? $"Erro ao inativar: {exInativar.Message}");
                }
            }
            else
            {
                LeitorConsole.ExibirInfo("Operação cancelada.");
            }
        }
    }

    // ──────────────────────── LEITURA DE DADOS ────────────────────────

    /// <summary>
    /// Lê todos os dados de um doador a partir do console.
    /// </summary>
    private static Doador LerDadosDoador()
    {
        var doador = new Doador();

        doador.Nome = LeitorConsole.LerTexto("Nome completo: ");
        doador.Cpf = LeitorConsole.LerTexto("CPF (somente números ou com pontuação): ");
        doador.DataNascimento = LeitorConsole.LerData("Data de nascimento (dd/MM/yyyy): ");

        doador.TipoSanguineo = LeitorConsole.LerTexto("Tipo sanguíneo (A, B, AB, O): ").ToUpper();
        doador.FatorRh = LeitorConsole.LerTexto("Fator Rh (+ ou -): ");
        doador.Sexo = LeitorConsole.LerTexto("Sexo (M ou F): ").ToUpper();

        doador.Peso = LeitorConsole.LerDecimal("Peso em kg (ex: 65,5): ");
        doador.Telefone = LeitorConsole.LerTextoOpcional("Telefone (opcional): ");
        doador.Email = LeitorConsole.LerTextoOpcional("E-mail (opcional): ");
        doador.Endereco = LeitorConsole.LerTextoOpcional("Endereço (opcional): ");

        doador.Ativo = true;

        return doador;
    }
}
