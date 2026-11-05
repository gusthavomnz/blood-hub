namespace BloodHub.Models;

public sealed class Doacao
{
    public int Id { get; set; }
    public int DoadorId { get; set; }
    public DateTime DataDoacao { get; set; }
    public decimal PesoKg { get; set; }
    public int VolumeMl { get; set; }
    public string Status { get; set; } = "Realizada";
    public string? MotivoRecusa { get; set; }
    public string LocalColeta { get; set; } = string.Empty;

    public Doador? Doador { get; set; }
}
