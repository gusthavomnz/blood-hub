using BloodHub.Modelos;
using BloodHub.Validacao;

namespace BloodHub.Servicos;

public sealed class ElegibilidadeService : IElegibilidadeService
{
    private const int IdadeMinima = 16;
    private const int IdadeMaxima = 69;
    private const decimal PesoMinimoKg = 50.0m;
    private const int IntervaloHomemDias = 60;
    private const int IntervaloMulherDias = 90;
    private const int LimiteAnualHomem = 4;
    private const int LimiteAnualMulher = 3;

    public ResultadoElegibilidade Verificar(
        Doador doador,
        List<Doacao> historicoDoacoes,
        DateTime dataDoacao,
        decimal pesoKg)
    {
        ArgumentNullException.ThrowIfNull(doador);
        ArgumentNullException.ThrowIfNull(historicoDoacoes);

        if (!doador.Ativo)
            return ResultadoElegibilidade.Reprovado("Doador inativo no sistema.");

        var motivos = new List<string>();

        var idade = Validadores.CalcularIdade(doador.DataNascimento, dataDoacao);
        if (idade < IdadeMinima)
            motivos.Add("O doador deve ter pelo menos 16 anos.");
        else if (idade > IdadeMaxima)
            motivos.Add("O doador deve ter no máximo 69 anos.");

        if (pesoKg < PesoMinimoKg)
            motivos.Add("O peso mínimo para doação é 50 kg.");

        var realizadasAnteriores = historicoDoacoes
            .Where(d => d.Status == "Realizada" && d.DataDoacao.Date < dataDoacao.Date)
            .OrderByDescending(d => d.DataDoacao)
            .ToList();

        var ultimaDoacao = realizadasAnteriores.FirstOrDefault();
        if (ultimaDoacao is not null)
        {
            var intervaloExigido = doador.Sexo == "M" ? IntervaloHomemDias : IntervaloMulherDias;
            var diasDesdeUltima = (dataDoacao.Date - ultimaDoacao.DataDoacao.Date).Days;

            if (diasDesdeUltima < intervaloExigido)
            {
                var diasRestantes = intervaloExigido - diasDesdeUltima;
                motivos.Add($"Intervalo mínimo entre doações não atingido. Aguarde mais {diasRestantes} dia(s).");
            }
        }

        var limiteAnual = doador.Sexo == "M" ? LimiteAnualHomem : LimiteAnualMulher;
        var inicioPeriodo = dataDoacao.AddDays(-365);
        var doacoesNoAno = historicoDoacoes.Count(d =>
            d.Status == "Realizada" &&
            d.DataDoacao.Date >= inicioPeriodo.Date &&
            d.DataDoacao.Date <= dataDoacao.Date);

        if (doacoesNoAno >= limiteAnual)
            motivos.Add($"Limite anual de doações atingido ({doacoesNoAno}/{limiteAnual} nos últimos 365 dias).");

        return motivos.Count == 0
            ? ResultadoElegibilidade.Aprovado()
            : ResultadoElegibilidade.Reprovado(motivos);
    }
}
