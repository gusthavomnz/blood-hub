using BloodHub.Data;
using BloodHub.Modelos;
using MySqlConnector;

namespace BloodHub.Repositorios;

/// <summary>
/// Persistência de doações via ADO.NET.
/// STUB — métodos serão implementados pelo Integrante 4.
/// Todos lançam NotImplementedException até a implementação real.
/// </summary>
public sealed class DoacaoRepositorio(ConnectionFactory factory) : IDoacaoRepositorio
{
    public Task<int> InserirAsync(Doacao doacao)
        => throw new NotImplementedException("DoacaoRepositorio.InserirAsync — aguardando Integrante 4.");

    public Task<bool> AtualizarAsync(Doacao doacao)
        => throw new NotImplementedException("DoacaoRepositorio.AtualizarAsync — aguardando Integrante 4.");

    public Task<bool> ExcluirAsync(int id)
        => throw new NotImplementedException("DoacaoRepositorio.ExcluirAsync — aguardando Integrante 4.");

    public Task<Doacao?> BuscarPorIdAsync(int id)
        => throw new NotImplementedException("DoacaoRepositorio.BuscarPorIdAsync — aguardando Integrante 4.");

    public Task<List<Doacao>> ListarComDoadorAsync()
        => throw new NotImplementedException("DoacaoRepositorio.ListarComDoadorAsync — aguardando Integrante 4.");

    public Task<List<Doacao>> ListarPorDoadorAsync(int doadorId)
        => throw new NotImplementedException("DoacaoRepositorio.ListarPorDoadorAsync — aguardando Integrante 4.");

    public Task<List<Doacao>> ListarRealizadasDoDoadorAsync(int doadorId)
        => throw new NotImplementedException("DoacaoRepositorio.ListarRealizadasDoDoadorAsync — aguardando Integrante 4.");

    public Task<List<Doacao>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim)
        => throw new NotImplementedException("DoacaoRepositorio.ListarPorPeriodoAsync — aguardando Integrante 4.");
}
