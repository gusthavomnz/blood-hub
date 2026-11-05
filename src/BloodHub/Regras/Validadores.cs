using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BloodHub.Models;

namespace BloodHub.Regras;

public static class Validadores
{
    private static readonly HashSet<string> TiposSanguineos =
        ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

    public static Doador NormalizarEValidar(Doador doador)
    {
        ArgumentNullException.ThrowIfNull(doador);

        var nome = (doador.Nome ?? string.Empty).Trim();
        if (nome.Length == 0)
            throw new ValidationException("Informe o nome do doador.");
        if (nome.Length > 120)
            throw new ValidationException("O nome deve ter no máximo 120 caracteres.");

        var cpf = NormalizarCpf(doador.Cpf);

        // A faixa de idade para doar pertence à elegibilidade, não ao cadastro.
        CalcularIdade(doador.DataNascimento, DateTime.Today);
        if (doador.DataNascimento.Year < 1000)
            throw new ValidationException("A data de nascimento deve ser a partir de 01/01/1000, limite do tipo DATE no MySQL.");

        var sexo = (doador.Sexo ?? string.Empty).Trim().ToUpperInvariant();
        if (sexo is not ("M" or "F"))
            throw new ValidationException("Informe M ou F para o sexo, conforme o cadastro.");

        var tipoSanguineo = (doador.TipoSanguineo ?? string.Empty).Trim().ToUpperInvariant();
        if (!TiposSanguineos.Contains(tipoSanguineo))
            throw new ValidationException("Tipo sanguíneo inválido. Use A+, A-, B+, B-, AB+, AB-, O+ ou O-.");

        var telefone = string.IsNullOrWhiteSpace(doador.Telefone) ? null : doador.Telefone.Trim();
        if (telefone is { Length: > 20 })
            throw new ValidationException("O telefone deve ter no máximo 20 caracteres.");

        return new Doador
        {
            Id = doador.Id,
            Nome = nome,
            Cpf = cpf,
            DataNascimento = doador.DataNascimento.Date,
            Sexo = sexo,
            TipoSanguineo = tipoSanguineo,
            Telefone = telefone,
            Ativo = doador.Ativo,
            Doacoes = doador.Doacoes,
        };
    }

    public static string NormalizarCpf(string? cpf)
    {
        if (!TentarNormalizarCpf(cpf, out var normalizado))
            throw new ValidationException("CPF inválido. Informe 11 dígitos válidos, com ou sem a máscara 000.000.000-00.");

        return normalizado;
    }

    public static bool CpfValido(string? cpf) => TentarNormalizarCpf(cpf, out _);

    public static int CalcularIdade(DateTime nascimento, DateTime referencia)
    {
        nascimento = nascimento.Date;
        referencia = referencia.Date;

        if (nascimento == DateTime.MinValue.Date)
            throw new ValidationException("Informe uma data de nascimento válida.");
        if (nascimento > referencia)
            throw new ValidationException("A data de nascimento não pode ser posterior à data de referência.");

        var idade = referencia.Year - nascimento.Year;
        if (nascimento.AddYears(idade) > referencia)
            idade--;

        return idade;
    }

    private static bool TentarNormalizarCpf(string? cpf, out string normalizado)
    {
        normalizado = string.Empty;
        var entrada = cpf?.Trim();
        if (string.IsNullOrEmpty(entrada))
            return false;

        if (!Regex.IsMatch(entrada, @"\A(?:[0-9]{11}|[0-9]{3}\.[0-9]{3}\.[0-9]{3}-[0-9]{2})\z"))
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
