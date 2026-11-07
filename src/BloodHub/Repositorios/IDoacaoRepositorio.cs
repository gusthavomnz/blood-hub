using BloodHub.Modelos;

namespace BloodHub.Repositorios;

/// <summary>
/// Contrato de persistência e consulta da entidade Doação via ADO.NET puro.
/// </summary>
public interface IDoacaoRepositorio
{
    // ── Persistência ──────────────────────────────────────────────────────────

    /// <summary>Insere uma nova doação e retorna o ID gerado.</summary>
    Task<int> InserirAsync(Doacao doacao);

    /// <summary>Atualiza os dados de uma doação existente.</summary>
    Task<bool> AtualizarAsync(Doacao doacao);

    /// <summary>Remove uma doação pelo ID.</summary>
    Task<bool> ExcluirAsync(int id);

    /// <summary>Busca uma doação pelo ID. Retorna null se não encontrada.</summary>
    Task<Doacao?> BuscarPorIdAsync(int id);

    // ── Consultas ─────────────────────────────────────────────────────────────

    /// <summary>Lista todas as doações com os dados do doador preenchidos via JOIN.</summary>
    Task<List<Doacao>> ListarComDoadorAsync();

    /// <summary>Lista todas as doações de um doador específico.</summary>
    Task<List<Doacao>> ListarPorDoadorAsync(int doadorId);

    /// <summary>
    /// Lista apenas as doações com Status "Realizada" de um doador,
    /// ordenadas da mais recente para a mais antiga. Usada pelo serviço de elegibilidade.
    /// </summary>
    Task<List<Doacao>> ListarRealizadasDoDoadorAsync(int doadorId);

    /// <summary>Lista doações dentro de um período (inclusive nas duas pontas).</summary>
    Task<List<Doacao>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim);
}
