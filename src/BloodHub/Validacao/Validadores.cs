using System.Text.RegularExpressions;
using BloodHub.Modelos;

namespace BloodHub.Validacao;

public static class Validadores
{
    public static List<string> ValidarDoador(Doador doador)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(doador.Nome))
            erros.Add("O nome é obrigatório.");
        else if (doador.Nome.Length < 3)
            erros.Add("O nome deve ter pelo menos 3 caracteres.");

        if (string.IsNullOrWhiteSpace(doador.Cpf))
            erros.Add("O CPF é obrigatório.");
        else if (!ValidarCpf(doador.Cpf))
            erros.Add("CPF inválido.");

        if (doador.DataNascimento == default)
            erros.Add("A data de nascimento é obrigatória.");
        else if (doador.DataNascimento > DateTime.Today)
            erros.Add("A data de nascimento não pode ser futura.");
        else if (CalcularIdade(doador.DataNascimento) < 16)
            erros.Add("O doador deve ter pelo menos 16 anos.");
        else if (CalcularIdade(doador.DataNascimento) > 69)
            erros.Add("O doador deve ter no máximo 69 anos.");

        string[] tiposValidos = ["A", "B", "AB", "O"];
        if (string.IsNullOrWhiteSpace(doador.TipoSanguineo) || !tiposValidos.Contains(doador.TipoSanguineo.ToUpper()))
            erros.Add("Tipo sanguíneo inválido. Use: A, B, AB ou O.");

        string[] fatoresValidos = ["+", "-"];
        if (string.IsNullOrWhiteSpace(doador.FatorRh) || !fatoresValidos.Contains(doador.FatorRh))
            erros.Add("Fator Rh inválido. Use: + ou -.");

        string[] sexosValidos = ["M", "F"];
        if (string.IsNullOrWhiteSpace(doador.Sexo) || !sexosValidos.Contains(doador.Sexo.ToUpper()))
            erros.Add("Sexo inválido. Use: M ou F.");

        if (doador.Peso < 50)
            erros.Add("O peso mínimo para doação é 50 kg.");

        if (string.IsNullOrWhiteSpace(doador.Telefone))
            erros.Add("O telefone é obrigatório.");

        if (!string.IsNullOrWhiteSpace(doador.Email) && !Regex.IsMatch(doador.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            erros.Add("E-mail inválido.");

        return erros;
    }

    public static bool ValidarCpf(string cpf)
    {
        cpf = Regex.Replace(cpf, @"[^\d]", "");

        if (cpf.Length != 11)
            return false;

        if (cpf.Distinct().Count() == 1)
            return false;

        int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        string tempCpf = cpf[..9];
        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        tempCpf += digito1;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return cpf.EndsWith($"{digito1}{digito2}");
    }

    public static string FormatarCpf(string cpf)
    {
        cpf = Regex.Replace(cpf, @"[^\d]", "");
        if (cpf.Length == 11)
            return $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}";
        return cpf;
    }

    private static int CalcularIdade(DateTime dataNascimento)
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - dataNascimento.Year;
        if (dataNascimento.Date > hoje.AddYears(-idade))
            idade--;
        return idade;
    }
}
