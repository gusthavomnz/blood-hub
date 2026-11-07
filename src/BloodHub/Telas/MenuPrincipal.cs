using BloodHub.Data;
using BloodHub.Repositorios;
using BloodHub.Servicos;

namespace BloodHub.Telas;

/// <summary>
/// Menu principal da aplicacao Blood Hub.
/// Integra todas as telas e opcoes disponiveis.
/// </summary>
public class MenuPrincipal
{
    private readonly TelaDoador _telaDoador;
    private readonly TelaDoacao _telaDoacao;
    private readonly DoadorRepositorio _doadorRepositorio;

    public MenuPrincipal(ConnectionFactory connectionFactory)
    {
        _doadorRepositorio = new DoadorRepositorio(connectionFactory);
        _telaDoador = new TelaDoador(_doadorRepositorio);

        var doacaoRepositorio = new DoacaoRepositorio(connectionFactory);
        var elegibilidadeService = new ElegibilidadeService();
        _telaDoacao = new TelaDoacao(doacaoRepositorio, _doadorRepositorio, elegibilidadeService);
    }

    /// <summary>
    /// Exibe o menu principal e processa a opcao escolhida em loop.
    /// </summary>
    public async Task ExecutarAsync()
    {
        ExibirBanner();

        while (true)
        {
            var opcao = LeitorConsole.LerOpcaoMenu("Menu Principal - Blood Hub", [
                "Gerenciar Doadores",
                "Gerenciar Doacoes",
                "Relatorios",
                "Sair"
            ]);

            try
            {
                switch (opcao)
                {
                    case 1:
                        await _telaDoador.ExibirMenuAsync();
                        break;
                    case 2:
                        await _telaDoacao.ExibirMenuAsync();
                        break;
                    case 3:
                        await ExibirMenuRelatoriosAsync();
                        break;
                    case 4:
                        Console.WriteLine();
                        LeitorConsole.ExibirInfo("Obrigado por usar o Blood Hub. Ate mais!");
                        return;
                }
            }
            catch (Exception ex)
            {
                LeitorConsole.ExibirErro($"Erro inesperado: {ex.Message}");
                LeitorConsole.AguardarEnter();
            }
        }
    }

    /// <summary>
    /// Submenu de relatorios — exibicao de dados em formato tabela.
    /// </summary>
    private async Task ExibirMenuRelatoriosAsync()
    {
        while (true)
        {
            var opcao = LeitorConsole.LerOpcaoMenu("Relatorios", [
                "Listar todos os doadores",
                "Listar doadores ativos",
                "Listar doadores inativos",
                "Doadores aptos a doar hoje",
                "Voltar ao menu principal"
            ]);

            try
            {
                switch (opcao)
                {
                    case 1:
                    {
                        var doadores = await _doadorRepositorio.ListarTodosAsync();
                        TelaRelatorios.ExibirTabelaDoadores(doadores);
                        break;
                    }
                    case 2:
                    {
                        var todos = await _doadorRepositorio.ListarTodosAsync();
                        var ativos = todos.Where(d => d.Ativo).ToList();
                        TelaRelatorios.ExibirTabelaDoadores(ativos);
                        break;
                    }
                    case 3:
                    {
                        var todos = await _doadorRepositorio.ListarTodosAsync();
                        var inativos = todos.Where(d => !d.Ativo).ToList();
                        TelaRelatorios.ExibirTabelaDoadores(inativos);
                        break;
                    }
                    case 4:
                        await _telaDoacao.ExibirDoadoresAptosHojeAsync();
                        break;
                    case 5:
                        return;
                }
            }
            catch (Exception ex)
            {
                LeitorConsole.ExibirErro($"Erro ao gerar relatorio: {ex.Message}");
            }

            LeitorConsole.AguardarEnter();
        }
    }

    private static void ExibirBanner()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine(@"  ██████╗ ██╗      ██████╗  ██████╗ ██████╗     ██╗  ██╗██╗   ██╗██████╗ ");
        Console.WriteLine(@"  ██╔══██╗██║     ██╔═══██╗██╔═══██╗██╔══██╗    ██║  ██║██║   ██║██╔══██╗");
        Console.WriteLine(@"  ██████╔╝██║     ██║   ██║██║   ██║██║  ██║    ███████║██║   ██║██████╔╝");
        Console.WriteLine(@"  ██╔══██╗██║     ██║   ██║██║   ██║██║  ██║    ██╔══██║██║   ██║██╔══██╗");
        Console.WriteLine(@"  ██████╔╝███████╗╚██████╔╝╚██████╔╝██████╔╝    ██║  ██║╚██████╔╝██████╔╝");
        Console.WriteLine(@"  ╚═════╝ ╚══════╝ ╚═════╝  ╚═════╝ ╚═════╝     ╚═╝  ╚═╝ ╚═════╝ ╚═════╝ ");
        Console.ResetColor();
        Console.WriteLine("  Sistema de Gerenciamento de Doadores de Sangue");
        Console.WriteLine();
    }
}
