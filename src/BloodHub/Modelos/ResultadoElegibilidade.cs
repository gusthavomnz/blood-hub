namespace BloodHub.Modelos;

/// <summary>
/// Representa o parecer de aptidão de um doador para realizar uma doação.
/// Use os métodos de fábrica <see cref="Aprovado"/> e <see cref="Reprovado(string[])"/>
/// para criar instâncias — nunca instancie diretamente.
/// </summary>
public class ResultadoElegibilidade
{
    public bool Apto { get; set; }
    public List<string> MotivosRecusa { get; set; } = [];

    public static ResultadoElegibilidade Aprovado() => new() { Apto = true };

    public static ResultadoElegibilidade Reprovado(params string[] motivos) => new()
    {
        Apto = false,
        MotivosRecusa = [.. motivos]
    };

    public static ResultadoElegibilidade Reprovado(List<string> motivos) => new()
    {
        Apto = false,
        MotivosRecusa = motivos
    };
}
