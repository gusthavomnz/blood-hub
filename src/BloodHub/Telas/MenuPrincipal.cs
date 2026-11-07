using BloodHub.Data;
using BloodHub.Repositorios;

namespace BloodHub.Telas;

/// <summary>
/// Menu principal da aplicação Blood Hub.
/// Integra todas as telas e opções disponíveis.
/// </summary>
public class MenuPrincipal
{
    private readonly TelaDoador _telaDoador;
    private readonly DoadorRepositorio _doadorRepositorio;

    public MenuPrincipal(ConnectionFactory connectionFactory)
    {
        _doadorRepositorio = new DoadorRepositorio(connectionFactory);
        _telaDoador = new TelaDoador(_doadorRepositorio);
    }

    /// <summary>
    /// Exibe o menu principal e processa a opção escolhida em loop.
    /// </summary>
    public async Task ExecutarAsync()
    {
        ExibirBanner();

        while (true)
        {
            var opcao = LeitorConsole.LerOpcaoMenu("Menu Principal — Blood Hub", [
                "Gerenciar Doadores",
                "Relatórios",
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
                        await ExibirMenuRelatoriosAsync();
                        break;
                    case 3:
                        Console.WriteLine();
                        LeitorConsole.ExibirInfo("Obrigado por usar o Blood Hub. Até mais!");
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
    /// Submenu de relatórios — exibição de dados em formato tabela.
    /// </summary>
    private async Task ExibirMenuRelatoriosAsync()
    {
        while (true)
        {
            var opcao = LeitorConsole.LerOpcaoMenu("Relatórios", [
                "Listar todos os doadores",
                "Listar doadores ativos",
                "Listar doadores inativos",
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
                        return;
                }
            }
            catch (Exception ex)
            {
                LeitorConsole.ExibirErro($"Erro ao gerar relatório: {ex.Message}");
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
