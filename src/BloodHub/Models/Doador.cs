namespace BloodHub.Models;

public sealed class Doador
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public string TipoSanguineo { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public bool Ativo { get; set; } = true;

    public List<Doacao> Doacoes { get; set; } = [];
}
