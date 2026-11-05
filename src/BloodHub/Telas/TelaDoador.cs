using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BloodHub.Data;
using BloodHub.Models;

namespace BloodHub.Telas;

public sealed class TelaDoador(DoadorRepositorio repositorio)
{
    public async Task ExecutarAsync()
    {
        while (true)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("Blood Hub — Doadores");
                Console.WriteLine("1. Cadastrar        2. Listar");
                Console.WriteLine("3. Buscar por ID    4. Buscar por CPF");
                Console.WriteLine("5. Buscar por nome  6. Atualizar");
                Console.WriteLine("7. Inativar         8. Excluir");
                Console.WriteLine("0. Sair (ou digite sair)");

                switch (LerLinha("Escolha uma opção: ").ToLowerInvariant())
                {
                    case "0":
                    case "sair":
                        return;
                    case "1":
                        var id = await repositorio.InserirAsync(LerCadastro());
                        Console.WriteLine($"Doador cadastrado com o ID {id}.");
                        break;
                    case "2":
                        MostrarLista(await repositorio.ListarAsync());
                        break;
                    case "3":
                        MostrarDoador(await repositorio.BuscarPorIdAsync(LerId()));
                        break;
                    case "4":
                        MostrarDoador(await repositorio.BuscarPorCpfAsync(LerLinha("CPF: ")));
                        break;
                    case "5":
                        MostrarLista(await repositorio.BuscarPorNomeAsync(LerLinha("Parte do nome: ")));
                        break;
                    case "6":
                        await AtualizarAsync();
                        break;
                    case "7":
                        Console.WriteLine(await repositorio.InativarAsync(LerId())
                            ? "Cadastro inativo. O histórico foi preservado."
                            : "Doador não encontrado.");
                        break;
                    case "8":
                        await ExcluirAsync();
                        break;
                    default:
                        Console.WriteLine("Opção inválida. Escolha um número de 0 a 8.");
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Dados inválidos: {ex.Message}");
            }
            catch (ErroBancoException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private async Task AtualizarAsync()
    {
        var doador = await repositorio.BuscarPorIdAsync(LerId());
        if (doador is null)
        {
            Console.WriteLine("Doador não encontrado.");
            return;
        }

        Console.WriteLine("Pressione Enter para manter cada valor atual. No telefone, digite - para limpar.");
        LerCadastro(doador);
        Console.WriteLine(await repositorio.AtualizarAsync(doador)
            ? "Cadastro atualizado."
            : "Doador não encontrado.");
    }

    private async Task ExcluirAsync()
    {
        var doador = await repositorio.BuscarPorIdAsync(LerId());
        if (doador is null)
        {
            Console.WriteLine("Doador não encontrado.");
            return;
        }

        var confirmacao = LerLinha($"Excluir o cadastro de {doador.Nome}? [s/N]: ");
        if (!confirmacao.Equals("s", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Exclusão cancelada.");
            return;
        }

        Console.WriteLine(await repositorio.ExcluirAsync(doador.Id)
            ? "Doador excluído."
            : "Doador não encontrado.");
    }

    private static Doador LerCadastro(Doador? existente = null)
    {
        var doador = existente ?? new Doador();
        doador.Nome = LerTexto("Nome", existente?.Nome);
        doador.Cpf = LerTexto("CPF", existente?.Cpf);
        doador.DataNascimento = LerData(existente?.DataNascimento);
        doador.Sexo = LerTexto("Sexo (M/F)", existente?.Sexo);
        doador.TipoSanguineo = LerTexto("Tipo sanguíneo (A+, A-, B+, B-, AB+, AB-, O+, O-)", existente?.TipoSanguineo);
        var telefone = LerTexto("Telefone (opcional)", existente?.Telefone);
        doador.Telefone = telefone is "" or "-" ? null : telefone;
        return doador;
    }

    private static string LerTexto(string rotulo, string? atual = null)
    {
        var entrada = LerLinha(atual is null ? $"{rotulo}: " : $"{rotulo} [{atual}]: ");
        return entrada.Length == 0 && atual is not null ? atual : entrada;
    }

    private static DateTime LerData(DateTime? atual)
    {
        while (true)
        {
            var entrada = LerLinha(atual is null
                ? "Nascimento (dd/MM/aaaa): "
                : $"Nascimento (dd/MM/aaaa) [{atual.Value:dd/MM/yyyy}]: ");
            if (entrada.Length == 0 && atual.HasValue)
                return atual.Value;
            if (DateTime.TryParseExact(entrada, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var nascimento))
                return nascimento;
            Console.WriteLine("Informe uma data válida no formato dd/MM/aaaa.");
        }
    }

    private static int LerId()
    {
        while (true)
        {
            if (int.TryParse(LerLinha("ID do doador: "), out var id) && id > 0)
                return id;
            Console.WriteLine("Informe um número inteiro maior que zero.");
        }
    }

    private static string LerLinha(string mensagem)
    {
        Console.Write(mensagem);
        return Console.ReadLine()?.Trim() ?? throw new OperationCanceledException();
    }

    private static void MostrarLista(List<Doador> doadores)
    {
        if (doadores.Count == 0)
        {
            Console.WriteLine("Nenhum doador encontrado.");
            return;
        }

        Console.WriteLine($"{"ID",-6} {"Nome",-30} {"CPF",-11} {"Tipo",-4} Ativo");
        foreach (var doador in doadores)
        {
            var nome = doador.Nome.Length > 30 ? doador.Nome[..27] + "..." : doador.Nome;
            Console.WriteLine($"{doador.Id,-6} {nome,-30} {doador.Cpf,-11} {doador.TipoSanguineo,-4} {(doador.Ativo ? "Sim" : "Não")}");
        }
    }

    private static void MostrarDoador(Doador? doador)
    {
        if (doador is null)
        {
            Console.WriteLine("Doador não encontrado.");
            return;
        }

        Console.WriteLine($"ID: {doador.Id} | Nome: {doador.Nome} | CPF: {doador.Cpf}");
        Console.WriteLine($"Nascimento: {doador.DataNascimento:dd/MM/yyyy} | Sexo: {doador.Sexo} | Tipo: {doador.TipoSanguineo}");
        Console.WriteLine($"Telefone: {doador.Telefone ?? "Não informado"} | Ativo: {(doador.Ativo ? "Sim" : "Não")}");
    }
}
