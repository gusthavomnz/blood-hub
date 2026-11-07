using System.Globalization;

namespace BloodHub.Telas;

/// <summary>
/// Classe auxiliar estática para leitura tolerante a falhas do terminal.
/// Repete a pergunta em loop até receber uma entrada válida.
/// </summary>
public static class LeitorConsole
{
    /// <summary>
    /// Lê uma string não vazia do console. Repete até o usuário digitar algo válido.
    /// </summary>
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

    /// <summary>
    /// Lê uma string do console, permitindo valor vazio (retorna null se vazio).
    /// Útil para campos opcionais.
    /// </summary>
    public static string? LerTextoOpcional(string prompt)
    {
        Console.Write(prompt);
        var entrada = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(entrada) ? null : entrada;
    }

    /// <summary>
    /// Lê um número inteiro do console. Repete até receber uma entrada válida.
    /// </summary>
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

    /// <summary>
    /// Lê um número decimal do console. Repete até receber uma entrada válida.
    /// </summary>
    public static decimal LerDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var entrada = Console.ReadLine()?.Trim();

            if (decimal.TryParse(entrada, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"), out decimal valor))
                return valor;

            // Tenta também com ponto como separador decimal
            if (decimal.TryParse(entrada, NumberStyles.Any, CultureInfo.InvariantCulture, out valor))
                return valor;

            ExibirErro("Valor inválido. Digite um número decimal (ex: 65,5 ou 65.5).");
        }
    }

    /// <summary>
    /// Lê uma data no formato dd/MM/yyyy do console. Repete até receber uma entrada válida.
    /// </summary>
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

    /// <summary>
    /// Exibe um menu numerado e lê a opção escolhida pelo usuário.
    /// Repete até receber uma opção válida dentro do intervalo.
    /// </summary>
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

    /// <summary>
    /// Pede uma confirmação S/N ao usuário. Retorna true se confirmar.
    /// </summary>
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

    /// <summary>
    /// Exibe uma mensagem de erro em destaque (amarelo sobre fundo escuro).
    /// </summary>
    public static void ExibirErro(string mensagem)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠ {mensagem}");
        Console.ResetColor();
    }

    /// <summary>
    /// Exibe uma mensagem de sucesso em destaque (verde).
    /// </summary>
    public static void ExibirSucesso(string mensagem)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✔ {mensagem}");
        Console.ResetColor();
    }

    /// <summary>
    /// Exibe uma mensagem informativa (ciano).
    /// </summary>
    public static void ExibirInfo(string mensagem)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  ℹ {mensagem}");
        Console.ResetColor();
    }

    /// <summary>
    /// Pausa a execução aguardando o usuário pressionar Enter.
    /// </summary>
    public static void AguardarEnter()
    {
        Console.WriteLine();
        Console.Write("Pressione Enter para continuar...");
        Console.ReadLine();
    }
}
