using System.ComponentModel.DataAnnotations;
using BloodHub.Modelos;
using BloodHub.Repositorios;
using BloodHub.Servicos;
using BloodHub.Validacao;
using MySqlConnector;

namespace BloodHub.Telas;

public class TelaDoacao
{
    private readonly IDoacaoRepositorio _doacaoRepositorio;
    private readonly DoadorRepositorio _doadorRepositorio;
    private readonly IElegibilidadeService _elegibilidadeService;

    public TelaDoacao(
        IDoacaoRepositorio doacaoRepositorio,
        DoadorRepositorio doadorRepositorio,
        IElegibilidadeService elegibilidadeService)
    {
        _doacaoRepositorio = doacaoRepositorio;
        _doadorRepositorio = doadorRepositorio;
        _elegibilidadeService = elegibilidadeService;
    }

    public async Task ExibirMenuAsync()
    {
        while (true)
        {
            var opcao = LeitorConsole.LerOpcaoMenu("Gerenciar Doações", [
                "Registrar doação",
                "Listar todas as doações",
                "Filtrar doações (por doador ou período)",
                "Atualizar doação",
                "Excluir doação",
                "Doadores aptos a doar hoje",
                "Voltar ao menu principal"
            ]);

            try
            {
                switch (opcao)
                {
                    case 1: await RegistrarAsync(); break;
                    case 2: await ListarTodasAsync(); break;
                    case 3: await FiltrarAsync(); break;
                    case 4: await AtualizarAsync(); break;
                    case 5: await ExcluirAsync(); break;
                    case 6: await ExibirDoadoresAptosHojeAsync(); break;
                    case 7: return;
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

    private async Task RegistrarAsync()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("══════ Registrar Nova Doação ══════");
        Console.ResetColor();

        var doador = await LocalizarDoadorAsync();
        if (doador is null) return;

        TelaRelatorios.ExibirDetalhesDoador(doador);
        Console.WriteLine();

        var dataDoacao = LeitorConsole.LerData("Data da doação (dd/MM/yyyy): ");
        var pesoKg = LeitorConsole.LerDecimal("Peso na triagem (kg, ex: 68,5): ");

        var volumeStr = LeitorConsole.LerTextoOpcional("Volume coletado em ml [Enter = 450]: ");
        var volumeMl = 450;
        if (volumeStr is not null && int.TryParse(volumeStr, out var v) && v > 0)
            volumeMl = v;

        var localColeta = LeitorConsole.LerTextoOpcional("Local de coleta [Enter = Hemocentro Central]: ")
                          ?? "Hemocentro Central";
        var observacao = LeitorConsole.LerTextoOpcional("Observação (opcional): ");

        var historico = await _doacaoRepositorio.ListarRealizadasDoDoadorAsync(doador.Id);
        var resultado = _elegibilidadeService.Verificar(doador, historico, dataDoacao, pesoKg);

        Console.WriteLine();

        if (resultado.Apto)
        {
            var doacao = new Doacao
            {
                DoadorId    = doador.Id,
                DataDoacao  = dataDoacao,
                PesoKg      = pesoKg,
                VolumeMl    = volumeMl,
                Status      = "Realizada",
                LocalColeta = localColeta,
                Observacao  = observacao
            };

            var id = await _doacaoRepositorio.InserirAsync(doacao);
            LeitorConsole.ExibirSucesso($"Doação registrada com sucesso! ID: {id}");
        }
        else
        {
            LeitorConsole.ExibirErro("Doador não está apto para doação:");
            foreach (var motivo in resultado.MotivosRecusa)
                LeitorConsole.ExibirErro($"  • {motivo}");

            Console.WriteLine();

            if (LeitorConsole.LerConfirmacao("Registrar como doação RECUSADA (para fins de histórico)?"))
            {
                var doacao = new Doacao
                {
                    DoadorId     = doador.Id,
                    DataDoacao   = dataDoacao,
                    PesoKg       = pesoKg,
                    VolumeMl     = 0,
                    Status       = "Recusada",
                    MotivoRecusa = string.Join("; ", resultado.MotivosRecusa),
                    LocalColeta  = localColeta,
                    Observacao   = observacao
                };

                var id = await _doacaoRepositorio.InserirAsync(doacao);
                LeitorConsole.ExibirInfo($"Doação recusada registrada no histórico. ID: {id}");
            }
            else
            {
                LeitorConsole.ExibirInfo("Operação cancelada. Nenhum registro foi salvo.");
            }
        }
    }

    private async Task ListarTodasAsync()
    {
        var doacoes = await _doacaoRepositorio.ListarComDoadorAsync();
        ExibirTabelaDoacoes(doacoes);
    }

    private async Task FiltrarAsync()
    {
        var opcao = LeitorConsole.LerOpcaoMenu("Filtrar Doações por", [
            "Doador (ID)",
            "Período (data início e fim)",
            "Voltar"
        ]);

        switch (opcao)
        {
            case 1: await FiltrarPorDoadorAsync(); break;
            case 2: await FiltrarPorPeriodoAsync(); break;
            case 3: return;
        }
    }

    private async Task FiltrarPorDoadorAsync()
    {
        var id = LeitorConsole.LerInt("ID do doador: ");
        var doador = await _doadorRepositorio.BuscarPorIdAsync(id);

        if (doador is null)
        {
            LeitorConsole.ExibirErro($"Doador com ID {id} não encontrado.");
            return;
        }

        LeitorConsole.ExibirInfo($"Doações de: {doador.Nome}");
        var doacoes = await _doacaoRepositorio.ListarPorDoadorAsync(id);
        ExibirTabelaDoacoes(doacoes);
    }

    private async Task FiltrarPorPeriodoAsync()
    {
        var inicio = LeitorConsole.LerData("Data início (dd/MM/yyyy): ");
        var fim    = LeitorConsole.LerData("Data fim    (dd/MM/yyyy): ");

        if (fim < inicio)
        {
            LeitorConsole.ExibirErro("A data fim não pode ser anterior à data início.");
            return;
        }

        var doacoes = await _doacaoRepositorio.ListarPorPeriodoAsync(inicio, fim);
        LeitorConsole.ExibirInfo($"Período: {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}");
        ExibirTabelaDoacoes(doacoes);
    }

    private async Task AtualizarAsync()
    {
        var id = LeitorConsole.LerInt("ID da doação a atualizar: ");
        var doacao = await _doacaoRepositorio.BuscarPorIdAsync(id);

        if (doacao is null)
        {
            LeitorConsole.ExibirErro($"Doação com ID {id} não encontrada.");
            return;
        }

        ExibirDetalhesDoacao(doacao);
        Console.WriteLine();
        LeitorConsole.ExibirInfo("Preencha os novos dados (Enter para manter o valor atual).");
        Console.WriteLine();

        var localColeta = LeitorConsole.LerTextoOpcional($"Local de coleta [{doacao.LocalColeta}]: ");
        if (localColeta is not null) doacao.LocalColeta = localColeta;

        var volumeStr = LeitorConsole.LerTextoOpcional($"Volume em ml [{doacao.VolumeMl}]: ");
        if (volumeStr is not null)
        {
            if (int.TryParse(volumeStr, out var volume) && volume >= 0)
                doacao.VolumeMl = volume;
            else
                LeitorConsole.ExibirErro("Volume inválido, mantendo o valor atual.");
        }

        var observacao = LeitorConsole.LerTextoOpcional($"Observação [{doacao.Observacao ?? ""}]: ");
        if (observacao is not null) doacao.Observacao = observacao;

        if (!LeitorConsole.LerConfirmacao("Confirma a atualização?"))
        {
            LeitorConsole.ExibirInfo("Atualização cancelada.");
            return;
        }

        var atualizado = await _doacaoRepositorio.AtualizarAsync(doacao);
        if (atualizado)
            LeitorConsole.ExibirSucesso("Doação atualizada com sucesso!");
        else
            LeitorConsole.ExibirErro("Nenhum registro foi alterado.");
    }

    private async Task ExcluirAsync()
    {
        var id = LeitorConsole.LerInt("ID da doação a excluir: ");
        var doacao = await _doacaoRepositorio.BuscarPorIdAsync(id);

        if (doacao is null)
        {
            LeitorConsole.ExibirErro($"Doação com ID {id} não encontrada.");
            return;
        }

        ExibirDetalhesDoacao(doacao);
        Console.WriteLine();

        if (!LeitorConsole.LerConfirmacao("Tem certeza que deseja EXCLUIR esta doação?"))
        {
            LeitorConsole.ExibirInfo("Operação cancelada.");
            return;
        }

        var excluido = await _doacaoRepositorio.ExcluirAsync(id);
        if (excluido)
            LeitorConsole.ExibirSucesso("Doação excluída com sucesso!");
        else
            LeitorConsole.ExibirErro("Nenhum registro foi removido.");
    }

    public async Task ExibirDoadoresAptosHojeAsync()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("══════ Doadores Aptos a Doar Hoje ══════");
        Console.ResetColor();

        LeitorConsole.ExibirInfo("Verificando elegibilidade de todos os doadores ativos...");
        Console.WriteLine();

        var todosDoadores = await _doadorRepositorio.ListarTodosAsync();
        var aptosHoje = new List<Doador>();

        foreach (var doador in todosDoadores.Where(d => d.Ativo))
        {
            var historico = await _doacaoRepositorio.ListarRealizadasDoDoadorAsync(doador.Id);
            var resultado = _elegibilidadeService.Verificar(doador, historico, DateTime.Today, doador.Peso);
            if (resultado.Apto)
                aptosHoje.Add(doador);
        }

        TelaRelatorios.ExibirTabelaDoadores(aptosHoje);

        if (aptosHoje.Count > 0)
            LeitorConsole.ExibirInfo($"{aptosHoje.Count} doador(es) apto(s) para doar hoje.");
    }

    private async Task<Doador?> LocalizarDoadorAsync()
    {
        var opcao = LeitorConsole.LerOpcaoMenu("Localizar Doador por", ["ID", "CPF"]);

        if (opcao == 1)
        {
            var id = LeitorConsole.LerInt("ID do doador: ");
            var doador = await _doadorRepositorio.BuscarPorIdAsync(id);
            if (doador is null)
                LeitorConsole.ExibirErro($"Doador com ID {id} não encontrado.");
            return doador;
        }
        else
        {
            var cpf = LeitorConsole.LerTexto("CPF do doador: ");
            cpf = Validadores.FormatarCpf(cpf);
            var doador = await _doadorRepositorio.BuscarPorCpfAsync(cpf);
            if (doador is null)
                LeitorConsole.ExibirErro($"Doador com CPF {cpf} não encontrado.");
            return doador;
        }
    }

    private static void ExibirTabelaDoacoes(List<Doacao> doacoes)
    {
        if (doacoes.Count == 0)
        {
            LeitorConsole.ExibirInfo("Nenhuma doação encontrada.");
            return;
        }

        const int colId     = 5;
        const int colDoador = 25;
        const int colData   = 12;
        const int colPeso   = 8;
        const int colVolume = 8;
        const int colStatus = 10;
        const int colLocal  = 22;

        var separador = MontarSeparador(colId, colDoador, colData, colPeso, colVolume, colStatus, colLocal);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(separador);
        Console.WriteLine(MontarLinha(
            colId, colDoador, colData, colPeso, colVolume, colStatus, colLocal,
            "ID", "Doador", "Data", "Peso(kg)", "Vol(ml)", "Status", "Local"));
        Console.WriteLine(separador);
        Console.ResetColor();

        foreach (var d in doacoes)
        {
            Console.ForegroundColor = d.Status == "Realizada" ? ConsoleColor.White : ConsoleColor.DarkGray;

            var nomeDoador = d.Doador?.Nome ?? $"ID {d.DoadorId}";
            Console.WriteLine(MontarLinha(
                colId, colDoador, colData, colPeso, colVolume, colStatus, colLocal,
                d.Id.ToString(),
                Truncar(nomeDoador, colDoador),
                d.DataDoacao.ToString("dd/MM/yyyy"),
                $"{d.PesoKg:F1}",
                d.VolumeMl.ToString(),
                d.Status,
                Truncar(d.LocalColeta, colLocal)));
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(separador);
        Console.ResetColor();

        Console.WriteLine($"  Total: {doacoes.Count} doação(ões)");
    }

    private static void ExibirDetalhesDoacao(Doacao d)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("══════ Detalhes da Doação ══════");
        Console.ResetColor();

        Console.WriteLine($"  ID...............: {d.Id}");
        Console.WriteLine($"  Doador...........: {(d.Doador is not null ? $"{d.Doador.Nome} (ID {d.DoadorId})" : $"ID {d.DoadorId}")}");
        Console.WriteLine($"  Data da Doação...: {d.DataDoacao:dd/MM/yyyy}");
        Console.WriteLine($"  Peso na Triagem..: {d.PesoKg:F1} kg");
        Console.WriteLine($"  Volume Coletado..: {d.VolumeMl} ml");
        Console.WriteLine($"  Status...........: {d.Status}");
        Console.WriteLine($"  Motivo Recusa....: {d.MotivoRecusa ?? "—"}");
        Console.WriteLine($"  Local de Coleta..: {d.LocalColeta}");
        Console.WriteLine($"  Observação.......: {d.Observacao ?? "—"}");
        Console.WriteLine($"  Registrado em....: {d.CriadoEm:dd/MM/yyyy HH:mm}");
    }

    private static string MontarSeparador(params int[] larguras)
    {
        var partes = larguras.Select(l => new string('-', l + 2));
        return $"+{string.Join("+", partes)}+";
    }

    private static string MontarLinha(
        int c1, int c2, int c3, int c4, int c5, int c6, int c7,
        string v1, string v2, string v3, string v4, string v5, string v6, string v7)
        => $"| {v1.PadRight(c1)} | {v2.PadRight(c2)} | {v3.PadRight(c3)} | {v4.PadRight(c4)} | {v5.PadRight(c5)} | {v6.PadRight(c6)} | {v7.PadRight(c7)} |";

    private static string Truncar(string texto, int max)
        => texto.Length <= max ? texto : texto[..(max - 1)] + "…";
}
