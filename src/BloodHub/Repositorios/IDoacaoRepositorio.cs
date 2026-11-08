using BloodHub.Modelos;

namespace BloodHub.Repositorios;

public interface IDoacaoRepositorio
{
    Task<int> InserirAsync(Doacao doacao);
    Task<bool> AtualizarAsync(Doacao doacao);
    Task<bool> ExcluirAsync(int id);
    Task<Doacao?> BuscarPorIdAsync(int id);
    Task<List<Doacao>> ListarComDoadorAsync();
    Task<List<Doacao>> ListarPorDoadorAsync(int doadorId);
    Task<List<Doacao>> ListarRealizadasDoDoadorAsync(int doadorId);
    Task<List<Doacao>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim);
}
