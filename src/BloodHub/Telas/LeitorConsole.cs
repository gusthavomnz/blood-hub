using System.Globalization;

namespace BloodHub.Telas;

public static class LeitorConsole
{
    public static string LerTexto(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var entrada = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(entrada))
                return entrada;

            ExibirErro("Entrada inválida. Digite um valor não vazio.");
        }
    }

    public static string? LerTextoOpcional(string prompt)
    {
        Console.Write(prompt);
        var entrada = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(entrada) ? null : entrada;
    }

    public static int LerInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var entrada = Console.ReadLine()?.Trim();

            if (int.TryParse(entrada, out int valor))
                return valor;

            ExibirErro("Valor inválido. Digite um número inteiro.");
        }
    }

    public static decimal LerDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var entrada = Console.ReadLine()?.Trim();

            if (decimal.TryParse(entrada, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"), out decimal valor))
                return valor;

            if (decimal.TryParse(entrada, NumberStyles.Any, CultureInfo.InvariantCulture, out valor))
                return valor;

            ExibirErro("Valor inválido. Digite um número decimal (ex: 65,5 ou 65.5).");
        }
    }

    public static DateTime LerData(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var entrada = Console.ReadLine()?.Trim();

            if (DateTime.TryParseExact(entrada, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data))
                return data;

            ExibirErro("Data inválida. Use o formato dd/MM/yyyy (ex: 25/12/1990).");
        }
    }

    public static int LerOpcaoMenu(string titulo, string[] opcoes)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"══════ {titulo} ══════");
        Console.ResetColor();

        for (int i = 0; i < opcoes.Length; i++)
        {
            Console.WriteLine($"  [{i + 1}] {opcoes[i]}");
        }

        Console.WriteLine();

        while (true)
        {
            Console.Write("Escolha uma opção: ");
            var entrada = Console.ReadLine()?.Trim();

            if (int.TryParse(entrada, out int opcao) && opcao >= 1 && opcao <= opcoes.Length)
                return opcao;

            ExibirErro($"Opção inválida. Digite um número entre 1 e {opcoes.Length}.");
        }
    }

    public static bool LerConfirmacao(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (S/N): ");
            var entrada = Console.ReadLine()?.Trim().ToUpper();

            if (entrada == "S") return true;
            if (entrada == "N") return false;

            ExibirErro("Digite S para Sim ou N para Não.");
        }
    }

    public static void ExibirErro(string mensagem)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠ {mensagem}");
        Console.ResetColor();
    }

    public static void ExibirSucesso(string mensagem)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✔ {mensagem}");
        Console.ResetColor();
    }

    public static void ExibirInfo(string mensagem)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  ℹ {mensagem}");
        Console.ResetColor();
    }

    public static void AguardarEnter()
    {
        Console.WriteLine();
        Console.Write("Pressione Enter para continuar...");
        Console.ReadLine();
    }
}
