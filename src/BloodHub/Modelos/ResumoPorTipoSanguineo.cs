namespace BloodHub.Modelos;

/// <summary>
/// Representa o resumo agregado de doações e volume coletado agrupados por tipo sanguíneo.
/// </summary>
public class ResumoPorTipoSanguineo
{
    public string TipoSanguineo { get; set; } = string.Empty;
    public int TotalDoacoes { get; set; }
    public long VolumeTotalMl { get; set; }
}
