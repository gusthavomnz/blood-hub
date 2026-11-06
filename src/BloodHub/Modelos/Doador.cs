namespace BloodHub.Modelos;

public class Doador
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string TipoSanguineo { get; set; } = string.Empty;
    public string FatorRh { get; set; } = string.Empty;
    public string Sexo { get; set; } = string.Empty;
    public decimal Peso { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
