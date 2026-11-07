using System.ComponentModel.DataAnnotations;
using BloodHub.Modelos;
using BloodHub.Validacao;

namespace BloodHub.Checks;

public static class ValidadoresChecks
{
    // Exemplo sintético com dois zeros iniciais e dígitos verificadores calculados.
    private const string CpfExemplo = "00123456797";

    public static int Executar()
    {
        var total = 0;
        var falhas = 0;

        void Verificar(string descricao, Action verificacao)
        {
            total++;
            try
            {
                verificacao();
                Console.WriteLine($"PASSOU: {descricao}");
            }
            catch (Exception ex)
            {
                falhas++;
                Console.Error.WriteLine($"FALHOU: {descricao}: {ex.Message}");
            }
        }

        Verificar("CPF válido preserva os zeros iniciais", () =>
        {
            Exigir(Validadores.ValidarCpf(CpfExemplo), "O CPF sintético deveria ser válido.");
            Exigir(Validadores.NormalizarCpf(CpfExemplo) == CpfExemplo, "Os zeros iniciais foram alterados.");
        });

        Verificar("CPF com máscara equivale ao CPF sem máscara", () =>
            Exigir(Validadores.NormalizarCpf("  001.234.567-97  ") == CpfExemplo, "A máscara não foi normalizada."));

        (string Descricao, string? Cpf)[] cpfsInvalidos =
        [
            ("valor nulo", null),
            ("texto vazio", "  "),
            ("menos de 11 dígitos", "0012345679"),
            ("mais de 11 dígitos", "000123456797"),
            ("letras no meio dos números", "001abc23456797"),
            ("primeiro dígito verificador incorreto", "00123456787"),
            ("segundo dígito verificador incorreto", "00123456798"),
            ("sequência de zeros", "00000000000"),
            ("sequência repetida", "11111111111"),
            ("máscara fora do padrão", "001-234-567.97"),
            ("espaço dentro do CPF", "001 23456797"),
            ("dígitos Unicode fora do padrão ASCII", "٠٠١٢٣٤٥٦٧٩٧"),
            ("máscara incompleta", "001.234.56797"),
        ];

        foreach (var caso in cpfsInvalidos)
        {
            Verificar($"CPF rejeita {caso.Descricao}", () =>
            {
                Exigir(!Validadores.ValidarCpf(caso.Cpf), "A consulta de validade deveria retornar false.");
                DeveLancar<ValidationException>(() => Validadores.NormalizarCpf(caso.Cpf));
            });
        }

        Verificar("cadastro normaliza texto, máscara, sexo, tipo e telefone", () =>
        {
            var original = DoadorValido();
            original.Nome = "  Maria de Teste  ";
            original.Cpf = "001.234.567-97";
            original.Sexo = " f ";
            original.TipoSanguineo = " ab ";
            original.FatorRh = " - ";
            original.Telefone = "  (85) 99999-0000  ";
            original.DataNascimento = new DateTime(1995, 5, 20, 15, 30, 0);

            var normalizado = Validadores.NormalizarEValidar(original);

            Exigir(normalizado.Nome == "Maria de Teste", "Nome não normalizado.");
            Exigir(normalizado.Cpf == CpfExemplo, "CPF não normalizado.");
            Exigir(normalizado.Sexo == "F", "Sexo não normalizado.");
            Exigir(normalizado.TipoSanguineo == "AB" && normalizado.FatorRh == "-", "Tipo sanguíneo não normalizado.");
            Exigir(normalizado.Telefone == "(85) 99999-0000", "Telefone não normalizado.");
            Exigir(normalizado.DataNascimento == new DateTime(1995, 5, 20), "Nascimento deveria guardar apenas a data.");
        });

        Verificar("normalização preserva ID, inativação e histórico sem modificar a entrada", () =>
        {
            var original = DoadorValido();
            original.Id = 42;
            original.Ativo = false;
            original.Nome = "  Maria de Teste  ";
            original.Doacoes.Add(new Doacao { Id = 7, DoadorId = 42 });

            var normalizado = Validadores.NormalizarEValidar(original);

            Exigir(!ReferenceEquals(original, normalizado), "Deveria retornar outro objeto.");
            Exigir(original.Nome == "  Maria de Teste  ", "O objeto de entrada foi modificado.");
            Exigir(normalizado.Id == 42, "ID alterado durante a normalização.");
            Exigir(!normalizado.Ativo, "Doador inativo foi reativado.");
            Exigir(normalizado.Doacoes.Count == 1 && normalizado.Doacoes[0].Id == 7, "Histórico perdido.");
        });

        Verificar("telefone vazio é opcional e vira null", () =>
        {
            var doador = DoadorValido();
            doador.Telefone = "   ";
            Exigir(Validadores.NormalizarEValidar(doador).Telefone is null, "Telefone vazio deveria ser null.");
        });

        Verificar("telefone aceita o limite da coluna e rejeita excesso", () =>
        {
            var doador = DoadorValido();
            doador.Telefone = new string('1', 20);
            Exigir(Validadores.NormalizarEValidar(doador).Telefone?.Length == 20, "Limite de 20 caracteres deveria ser aceito.");
            doador.Telefone += "1";
            DeveLancar<ValidationException>(() => Validadores.NormalizarEValidar(doador));
        });

        Verificar("nome rejeita espaços em branco", () =>
        {
            var doador = DoadorValido();
            doador.Nome = "   ";
            DeveLancar<ValidationException>(() => Validadores.NormalizarEValidar(doador));
        });

        Verificar("nome aceita o limite da coluna e rejeita excesso", () =>
        {
            var doador = DoadorValido();
            doador.Nome = new string('A', 120);
            Exigir(Validadores.NormalizarEValidar(doador).Nome.Length == 120, "Limite de 120 caracteres deveria ser aceito.");
            doador.Nome += "A";
            DeveLancar<ValidationException>(() => Validadores.NormalizarEValidar(doador));
        });

        Verificar("sexo fora dos valores do modelo é rejeitado", () =>
        {
            var doador = DoadorValido();
            doador.Sexo = "X";
            DeveLancar<ValidationException>(() => Validadores.NormalizarEValidar(doador));
        });

        Verificar("todos os oito tipos sanguíneos são aceitos", () =>
        {
            foreach (var (tipo, fator) in new[]
            {
                ("A", "+"), ("A", "-"), ("B", "+"), ("B", "-"),
                ("AB", "+"), ("AB", "-"), ("O", "+"), ("O", "-")
            })
            {
                var doador = DoadorValido();
                doador.TipoSanguineo = tipo;
                doador.FatorRh = fator;
                var normalizado = Validadores.NormalizarEValidar(doador);
                Exigir(normalizado.TipoSanguineo == tipo && normalizado.FatorRh == fator, $"Tipo {tipo}{fator} rejeitado.");
            }
        });

        Verificar("tipo sanguíneo desconhecido é rejeitado", () =>
        {
            var doador = DoadorValido();
            doador.TipoSanguineo = "C";
            DeveLancar<ValidationException>(() => Validadores.NormalizarEValidar(doador));
        });

        Verificar("nascimento no futuro é rejeitado no cadastro", () =>
        {
            var doador = DoadorValido();
            doador.DataNascimento = DateTime.Today.AddDays(1);
            DeveLancar<ValidationException>(() => Validadores.NormalizarEValidar(doador));
        });

        Verificar("nascimento não preenchido é rejeitado", () =>
        {
            var doador = DoadorValido();
            doador.DataNascimento = default;
            DeveLancar<ValidationException>(() => Validadores.NormalizarEValidar(doador));
        });

        Verificar("cadastro rejeita datas anteriores ao limite DATE do MySQL", () =>
        {
            foreach (var nascimento in new[] { new DateTime(999, 12, 31), new DateTime(1, 1, 2) })
            {
                var doador = DoadorValido();
                doador.DataNascimento = nascimento;
                DeveLancar<ValidationException>(() => Validadores.NormalizarEValidar(doador));
            }
        });

        Verificar("cadastro aceita o limite inicial do tipo DATE do MySQL", () =>
        {
            var doador = DoadorValido();
            doador.DataNascimento = new DateTime(1000, 1, 1);
            Exigir(Validadores.NormalizarEValidar(doador).DataNascimento == doador.DataNascimento, "O limite inicial de armazenamento deveria ser aceito.");
        });

        Verificar("idade muda no aniversário, sem contar dias divididos por 365", () =>
        {
            var nascimento = new DateTime(2000, 11, 5);
            Exigir(Validadores.CalcularIdade(nascimento, new DateTime(2026, 11, 4)) == 25, "Idade incorreta antes do aniversário.");
            Exigir(Validadores.CalcularIdade(nascimento, new DateTime(2026, 11, 5)) == 26, "Idade incorreta no aniversário.");
            Exigir(Validadores.CalcularIdade(nascimento, new DateTime(2026, 11, 6)) == 26, "Idade incorreta depois do aniversário.");
        });

        Verificar("idade considera aniversário em 29 de fevereiro", () =>
        {
            var nascimento = new DateTime(2000, 2, 29);
            Exigir(Validadores.CalcularIdade(nascimento, new DateTime(2024, 2, 28)) == 23, "Idade incorreta antes de 29/02.");
            Exigir(Validadores.CalcularIdade(nascimento, new DateTime(2024, 2, 29)) == 24, "Idade incorreta em 29/02.");
        });

        Verificar("cálculo de idade rejeita nascimento posterior à referência", () =>
            DeveLancar<ValidationException>(() => Validadores.CalcularIdade(new DateTime(2000, 1, 2), new DateTime(2000, 1, 1))));

        Verificar("cadastro não aplica a faixa clínica de elegibilidade", () =>
        {
            var doador = DoadorValido();
            doador.DataNascimento = DateTime.Today.AddYears(-80);
            Exigir(Validadores.NormalizarEValidar(doador).DataNascimento == doador.DataNascimento, "Cadastro deveria preservar histórico independentemente da elegibilidade atual.");
        });

        Console.WriteLine($"Verificações de validação: {total - falhas}/{total} passaram.");
        return falhas == 0 ? 0 : 1;
    }

    private static Doador DoadorValido() => new()
    {
        Nome = "Maria de Teste",
        Cpf = CpfExemplo,
        DataNascimento = new DateTime(1995, 5, 20),
        Sexo = "F",
        TipoSanguineo = "O",
        FatorRh = "+",
        Peso = 65m,
    };

    private static void Exigir(bool condicao, string mensagem)
    {
        if (!condicao)
            throw new InvalidOperationException(mensagem);
    }

    private static void DeveLancar<TException>(Action acao) where TException : Exception
    {
        try
        {
            acao();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException($"Era esperado o erro {typeof(TException).Name}.");
    }
}
