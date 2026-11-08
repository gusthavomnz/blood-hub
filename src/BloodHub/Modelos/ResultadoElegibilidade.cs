namespace BloodHub.Modelos;

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
