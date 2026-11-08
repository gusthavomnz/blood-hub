using BloodHub.Modelos;

namespace BloodHub.Repositorios;

public interface IRelatorioRepositorio
{
    Task<List<ResumoPorTipoSanguineo>> TotalPorTipoSanguineoAsync();
    Task<List<DoacoesPorMes>> DoacoesPorMesAsync();
}
