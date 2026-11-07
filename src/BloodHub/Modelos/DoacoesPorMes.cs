namespace BloodHub.Modelos;

/// <summary>
/// Representa a quantidade de doações e volume coletado agrupados por ano e mês.
/// </summary>
public class DoacoesPorMes
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public int TotalDoacoes { get; set; }
    public long VolumeTotalMl { get; set; }
}
