using BloodHub.Modelos;

namespace BloodHub.Telas;

/// <summary>
/// Métodos auxiliares para exibir listas e coleções de dados formatadas
/// em colunas alinhadas no terminal (estilo tabela com cabeçalhos e separadores).
/// </summary>
public static class TelaRelatorios
{
    /// <summary>
    /// Exibe uma lista de doadores em formato de tabela alinhada no console.
    /// </summary>
    public static void ExibirTabelaDoadores(List<Doador> doadores)
    {
        if (doadores.Count == 0)
        {
            LeitorConsole.ExibirInfo("Nenhum doador encontrado.");
            return;
        }

        // Definir larguras das colunas
        const int colId = 5;
        const int colNome = 30;
        const int colCpf = 15;
        const int colNasc = 12;
        const int colTipo = 6;
        const int colSexo = 5;
        const int colPeso = 8;
        const int colTelefone = 16;
        const int colAtivo = 7;

        var separador = MontarSeparador(colId, colNome, colCpf, colNasc, colTipo, colSexo, colPeso, colTelefone, colAtivo);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(separador);
        Console.WriteLine(MontarLinha(
            colId, colNome, colCpf, colNasc, colTipo, colSexo, colPeso, colTelefone, colAtivo,
            "ID", "Nome", "CPF", "Nascimento", "Tipo", "Sexo", "Peso", "Telefone", "Ativo"));
        Console.WriteLine(separador);
        Console.ResetColor();

        foreach (var d in doadores)
        {
            var corLinha = d.Ativo ? ConsoleColor.White : ConsoleColor.DarkGray;
            Console.ForegroundColor = corLinha;

            Console.WriteLine(MontarLinha(
                colId, colNome, colCpf, colNasc, colTipo, colSexo, colPeso, colTelefone, colAtivo,
                d.Id.ToString(),
                Truncar(d.Nome, colNome),
                d.Cpf,
                d.DataNascimento.ToString("dd/MM/yyyy"),
                $"{d.TipoSanguineo}{d.FatorRh}",
                d.Sexo,
                $"{d.Peso:F1}",
                d.Telefone,
                d.Ativo ? "Sim" : "Não"));
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(separador);
        Console.ResetColor();

        Console.WriteLine($"  Total: {doadores.Count} doador(es)");
    }

    /// <summary>
    /// Exibe os detalhes completos de um doador.
    /// </summary>
    public static void ExibirDetalhesDoador(Doador d)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("══════ Detalhes do Doador ══════");
        Console.ResetColor();

        Console.WriteLine($"  ID...............: {d.Id}");
        Console.WriteLine($"  Nome.............: {d.Nome}");
        Console.WriteLine($"  CPF..............: {d.Cpf}");
        Console.WriteLine($"  Data Nascimento..: {d.DataNascimento:dd/MM/yyyy}");
        Console.WriteLine($"  Tipo Sanguíneo...: {d.TipoSanguineo}{d.FatorRh}");
        Console.WriteLine($"  Sexo.............: {(d.Sexo == "M" ? "Masculino" : "Feminino")}");
        Console.WriteLine($"  Peso.............: {d.Peso:F1} kg");
        Console.WriteLine($"  Telefone.........: {d.Telefone}");
        Console.WriteLine($"  E-mail...........: {d.Email ?? "(não informado)"}");
        Console.WriteLine($"  Endereço.........: {d.Endereco ?? "(não informado)"}");
        Console.WriteLine($"  Ativo............: {(d.Ativo ? "Sim" : "Não")}");
        Console.WriteLine($"  Cadastrado em....: {d.CriadoEm:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"  Atualizado em....: {d.AtualizadoEm:dd/MM/yyyy HH:mm}");
    }

    /// <summary>
    /// Monta a linha separadora da tabela: +------+--------+...
    /// </summary>
    private static string MontarSeparador(params int[] larguras)
    {
        var partes = larguras.Select(l => new string('-', l + 2));
        return $"+{string.Join("+", partes)}+";
    }

    /// <summary>
    /// Monta uma linha da tabela com valores alinhados: | val1 | val2 | ...
    /// </summary>
    private static string MontarLinha(
        int c1, int c2, int c3, int c4, int c5, int c6, int c7, int c8, int c9,
        string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8, string v9)
    {
        return $"| {v1.PadRight(c1)} | {v2.PadRight(c2)} | {v3.PadRight(c3)} | {v4.PadRight(c4)} | {v5.PadRight(c5)} | {v6.PadRight(c6)} | {v7.PadRight(c7)} | {v8.PadRight(c8)} | {v9.PadRight(c9)} |";
    }

    /// <summary>
    /// Trunca uma string se ela exceder o tamanho máximo, adicionando "…" no final.
    /// </summary>
    private static string Truncar(string texto, int max)
    {
        if (texto.Length <= max) return texto;
        return texto[..(max - 1)] + "…";
    }
}
