using BloodHub.Repositorios;
using BloodHub.Telas;

namespace BloodHub.Controladores;

public sealed class RelatorioController(
    IDoadorRepositorio doadorRepositorio,
    IRelatorioRepositorio relatorioRepositorio,
    TelaDoacao telaDoacao)
{
    private readonly IDoadorRepositorio _doadorRepositorio =
        doadorRepositorio ?? throw new ArgumentNullException(nameof(doadorRepositorio));
    private readonly IRelatorioRepositorio _relatorioRepositorio =
        relatorioRepositorio ?? throw new ArgumentNullException(nameof(relatorioRepositorio));
    private readonly TelaDoacao _telaDoacao =
        telaDoacao ?? throw new ArgumentNullException(nameof(telaDoacao));

    public async Task ExecutarMenuAsync()
    {
        while (true)
        {
            var opcao = LeitorConsole.LerOpcaoMenu("Relatórios e Estatísticas", [
                "Listar todos os doadores",
                "Listar doadores ativos",
                "Listar doadores inativos",
                "Doadores aptos a doar hoje",
                "Total de doações e volume por tipo sanguíneo (GROUP BY)",
                "Doações realizadas por mês (GROUP BY)",
                "Voltar ao menu principal"
            ]);

            try
            {
                switch (opcao)
                {
                    case 1: await ListarTodosAsync(); break;
                    case 2: await ListarAtivosAsync(); break;
                    case 3: await ListarInativosAsync(); break;
                    case 4: await DoadoresAptosHojeAsync(); break;
                    case 5: await TotalPorTipoSanguineoAsync(); break;
                    case 6: await DoacoesPorMesAsync(); break;
                    case 7: return;
                }
            }
            catch (Exception ex)
            {
                LeitorConsole.ExibirErro($"Erro ao gerar relatório: {ex.Message}");
            }

            LeitorConsole.AguardarEnter();
        }
    }

    private async Task ListarTodosAsync()
    {
        var doadores = await _doadorRepositorio.ListarTodosAsync();
        TelaRelatorios.ExibirTabelaDoadores(doadores);
    }

    private async Task ListarAtivosAsync()
    {
        var todos = await _doadorRepositorio.ListarTodosAsync();
        var ativos = todos.Where(d => d.Ativo).ToList();
        TelaRelatorios.ExibirTabelaDoadores(ativos);
    }

    private async Task ListarInativosAsync()
    {
        var todos = await _doadorRepositorio.ListarTodosAsync();
        var inativos = todos.Where(d => !d.Ativo).ToList();
        TelaRelatorios.ExibirTabelaDoadores(inativos);
    }

    private Task DoadoresAptosHojeAsync()
        => _telaDoacao.ExibirDoadoresAptosHojeAsync();

    private async Task TotalPorTipoSanguineoAsync()
    {
        var dados = await _relatorioRepositorio.TotalPorTipoSanguineoAsync();
        TelaRelatorios.ExibirTabelaTipoSanguineo(dados);
    }

    private async Task DoacoesPorMesAsync()
    {
        var dados = await _relatorioRepositorio.DoacoesPorMesAsync();
        TelaRelatorios.ExibirTabelaDoacoesPorMes(dados);
    }
}
