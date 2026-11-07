using BloodHub.Modelos;

namespace BloodHub.Servicos;

/// <summary>
/// Contrato do serviço de verificação de aptidão para doação de sangue.
/// </summary>
public interface IElegibilidadeService
{
    /// <summary>
    /// Verifica se o doador está apto a realizar uma doação na data informada.
    /// </summary>
    /// <param name="doador">Dados cadastrais do doador.</param>
    /// <param name="historicoDoacoes">Histórico completo de doações do doador (realizadas e recusadas).</param>
    /// <param name="dataDoacao">Data em que a doação será realizada.</param>
    /// <param name="pesoKg">Peso aferido na triagem em quilogramas.</param>
    /// <returns>Resultado com aptidão e lista de motivos de recusa, se houver.</returns>
    ResultadoElegibilidade Verificar(Doador doador, List<Doacao> historicoDoacoes, DateTime dataDoacao, decimal pesoKg);
}
