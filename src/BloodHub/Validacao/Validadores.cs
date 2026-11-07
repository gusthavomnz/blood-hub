using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BloodHub.Modelos;

namespace BloodHub.Validacao;

public static class Validadores
{
    private static readonly string[] TiposSanguineos = ["A", "B", "AB", "O"];

    public static List<string> ValidarDoador(Doador doador)
    {
        ArgumentNullException.ThrowIfNull(doador);
        var erros = new List<string>();
        var nome = doador.Nome?.Trim() ?? string.Empty;

        if (nome.Length == 0)
            erros.Add("O nome é obrigatório.");
        else if (nome.Length > 120)
            erros.Add("O nome deve ter no máximo 120 caracteres.");

        if (!ValidarCpf(doador.Cpf))
            erros.Add("CPF inválido. Informe 11 dígitos válidos, com ou sem pontuação.");

        if (doador.DataNascimento == default)
            erros.Add("A data de nascimento é obrigatória.");
        else if (doador.DataNascimento.Date > DateTime.Today)
            erros.Add("A data de nascimento não pode ser futura.");
        else if (doador.DataNascimento.Year < 1000)
            erros.Add("A data de nascimento deve ser a partir de 01/01/1000.");

        var tipo = doador.TipoSanguineo?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!TiposSanguineos.Contains(tipo, StringComparer.Ordinal))
            erros.Add("Tipo sanguíneo inválido. Use A, B, AB ou O.");

        var fatorRh = doador.FatorRh?.Trim();
        if (fatorRh is not ("+" or "-"))
            erros.Add("Fator Rh inválido. Use + ou -.");

        if (doador.Sexo?.Trim().ToUpperInvariant() is not ("M" or "F"))
            erros.Add("Sexo inválido. Use M ou F.");

        if (doador.Peso <= 0 || doador.Peso > 999.99m)
            erros.Add("O peso deve ser maior que zero e caber no limite de armazenamento.");

        if (!string.IsNullOrWhiteSpace(doador.Telefone) && doador.Telefone.Trim().Length > 20)
            erros.Add("O telefone deve ter no máximo 20 caracteres.");

        if (!string.IsNullOrWhiteSpace(doador.Email))
        {
            var email = doador.Email.Trim();
            if (email.Length > 150 || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                erros.Add("E-mail inválido ou maior que 150 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(doador.Endereco) && doador.Endereco.Trim().Length > 255)
            erros.Add("O endereço deve ter no máximo 255 caracteres.");

        return erros;
    }

    public static Doador NormalizarEValidar(Doador doador)
    {
        var erros = ValidarDoador(doador);
        if (erros.Count > 0)
            throw new ValidationException(string.Join(Environment.NewLine, erros));

        return new Doador
        {
            Id = doador.Id,
            Nome = doador.Nome.Trim(),
            Cpf = NormalizarCpf(doador.Cpf),
            DataNascimento = doador.DataNascimento.Date,
            TipoSanguineo = doador.TipoSanguineo.Trim().ToUpperInvariant(),
            FatorRh = doador.FatorRh.Trim(),
            Sexo = doador.Sexo.Trim().ToUpperInvariant(),
            Peso = decimal.Round(doador.Peso, 2),
            Telefone = string.IsNullOrWhiteSpace(doador.Telefone) ? null : doador.Telefone.Trim(),
            Email = string.IsNullOrWhiteSpace(doador.Email) ? null : doador.Email.Trim(),
            Endereco = string.IsNullOrWhiteSpace(doador.Endereco) ? null : doador.Endereco.Trim(),
            Ativo = doador.Ativo,
            CriadoEm = doador.CriadoEm,
            AtualizadoEm = doador.AtualizadoEm,
            Doacoes = [.. doador.Doacoes]
        };
    }

    public static bool ValidarCpf(string? cpf) => TentarNormalizarCpf(cpf, out _);

    // Retorna os 11 dígitos, preservando zeros à esquerda para armazenamento e busca.
    public static string FormatarCpf(string? cpf) => NormalizarCpf(cpf);

    public static string NormalizarCpf(string? cpf)
    {
        if (!TentarNormalizarCpf(cpf, out var normalizado))
            throw new ValidationException("CPF inválido. Informe 11 dígitos válidos, com ou sem pontuação.");
        return normalizado;
    }

    public static int CalcularIdade(DateTime dataNascimento, DateTime referencia)
    {
        var nascimento = dataNascimento.Date;
        var data = referencia.Date;
        if (nascimento == DateTime.MinValue.Date || nascimento > data)
            throw new ValidationException("Informe uma data de nascimento válida, que não seja futura.");

        var idade = data.Year - nascimento.Year;
        if (nascimento.AddYears(idade) > data)
            idade--;
        return idade;
    }

    private static bool TentarNormalizarCpf(string? cpf, out string normalizado)
    {
        normalizado = string.Empty;
        var entrada = cpf?.Trim();
        if (string.IsNullOrEmpty(entrada) ||
            !Regex.IsMatch(entrada, @"\A(?:[0-9]{11}|[0-9]{3}\.[0-9]{3}\.[0-9]{3}-[0-9]{2})\z"))
            return false;

        var digitos = entrada.Replace(".", string.Empty).Replace("-", string.Empty);
        if (digitos.All(digito => digito == digitos[0]))
            return false;

        for (var quantidade = 9; quantidade <= 10; quantidade++)
        {
            var soma = 0;
            for (var indice = 0; indice < quantidade; indice++)
                soma += (digitos[indice] - '0') * (quantidade + 1 - indice);

            var verificador = 11 - soma % 11;
            if (verificador >= 10)
                verificador = 0;
            if (digitos[quantidade] - '0' != verificador)
                return false;
        }

        normalizado = digitos;
        return true;
    }
}
