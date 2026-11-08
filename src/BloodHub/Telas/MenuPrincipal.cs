using BloodHub.Controladores;
using BloodHub.Data;
using BloodHub.Repositorios;
using BloodHub.Servicos;

namespace BloodHub.Telas;

public class MenuPrincipal
{
    private readonly DoadorController _doadorController;
    private readonly TelaDoacao _telaDoacao;
    private readonly RelatorioController _relatorioController;

    public MenuPrincipal(ConnectionFactory connectionFactory)
    {
        var doadorRepositorio   = new DoadorRepositorio(connectionFactory);
        var doacaoRepositorio   = new DoacaoRepositorio(connectionFactory);
        var relatorioRepositorio = new RelatorioRepositorio(connectionFactory);
        var elegibilidadeService = new ElegibilidadeService();

        var telaDoador = new TelaDoador();
        _telaDoacao    = new TelaDoacao(doacaoRepositorio, doadorRepositorio, elegibilidadeService);

        _doadorController    = new DoadorController(doadorRepositorio, telaDoador);
        _relatorioController = new RelatorioController(doadorRepositorio, relatorioRepositorio, _telaDoacao);
    }

    public async Task ExecutarAsync()
    {
        ExibirBanner();

        while (true)
        {
            var opcao = LeitorConsole.LerOpcaoMenu("Menu Principal — Blood Hub", [
                "Gerenciar Doadores",
                "Gerenciar Doações",
                "Relatórios e Estatísticas",
                "Sair"
            ]);

            try
            {
                switch (opcao)
                {
                    case 1: await _doadorController.ExecutarMenuAsync(); break;
                    case 2: await _telaDoacao.ExibirMenuAsync(); break;
                    case 3: await _relatorioController.ExecutarMenuAsync(); break;
                    case 4:
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
