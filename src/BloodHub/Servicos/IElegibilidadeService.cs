using BloodHub.Modelos;

namespace BloodHub.Servicos;

public interface IElegibilidadeService
{
    ResultadoElegibilidade Verificar(Doador doador, List<Doacao> historicoDoacoes, DateTime dataDoacao, decimal pesoKg);
}
