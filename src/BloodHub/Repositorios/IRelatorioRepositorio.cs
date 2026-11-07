using BloodHub.Modelos;

namespace BloodHub.Repositorios;

/// <summary>
/// Contrato para consultas analíticas e relatórios agregados com ADO.NET puro.
/// </summary>
public interface IRelatorioRepositorio
{
    Task<List<ResumoPorTipoSanguineo>> TotalPorTipoSanguineoAsync();
    Task<List<DoacoesPorMes>> DoacoesPorMesAsync();
}
