using BloodHub.Modelos;

namespace BloodHub.Repositorios;

/// <summary>
/// Contrato para operações de persistência e consulta da entidade Doacao.
/// </summary>
public interface IDoacaoRepositorio
{
    Task<int> InserirAsync(Doacao doacao);
    Task<bool> AtualizarAsync(Doacao doacao);
    Task<bool> ExcluirAsync(int id);
    Task<Doacao?> BuscarPorIdAsync(int id);
    Task<List<Doacao>> ListarComDoadorAsync();
    Task<List<Doacao>> ListarPorDoadorAsync(int doadorId);
    Task<List<Doacao>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<List<Doacao>> ListarRealizadasDoDoadorAsync(int doadorId);
}
