using BloodHub.Modelos;

namespace BloodHub.Repositorios;

public interface IDoadorRepositorio
{
    Task<int> InserirAsync(Doador doador);
    Task<List<Doador>> ListarTodosAsync();
    Task<Doador?> BuscarPorIdAsync(int id);
    Task<Doador?> BuscarPorCpfAsync(string cpf);
    Task<List<Doador>> BuscarPorNomeAsync(string nome);
    Task<bool> AtualizarAsync(Doador doador);
    Task<bool> InativarAsync(int id);
    Task<bool> ExcluirAsync(int id);
}
