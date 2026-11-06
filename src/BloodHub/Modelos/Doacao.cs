namespace BloodHub.Modelos;

public class Doacao
{
    public int Id { get; set; }
    public int DoadorId { get; set; }
    public DateTime DataDoacao { get; set; }
    public int VolumeMl { get; set; }
    public string? Observacao { get; set; }
    public DateTime CriadoEm { get; set; }
}
