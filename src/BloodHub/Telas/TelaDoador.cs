using BloodHub.Modelos;
using BloodHub.Validacao;

namespace BloodHub.Telas;

public class TelaDoador
{
    public int ExibirMenu()
    {
        return LeitorConsole.LerOpcaoMenu("Gerenciar Doadores", [
            "Cadastrar doador",
            "Listar doadores",
            "Buscar doador",
            "Atualizar doador",
            "Excluir doador",
            "Voltar ao menu principal"
        ]);
    }

    public int ExibirMenuBusca()
    {
        return LeitorConsole.LerOpcaoMenu("Buscar Doador por", [
            "ID",
            "CPF",
            "Nome",
            "Voltar"
        ]);
    }

    public void ExibirCabecalhoCadastro()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("══════ Cadastrar Novo Doador ══════");
        Console.ResetColor();
    }

    public void ExibirAvisoAtualizacao()
    {
        Console.WriteLine();
        LeitorConsole.ExibirInfo("Preencha os novos dados (deixe em branco para manter o valor atual).");
        Console.WriteLine();
    }

    public Doador LerDadosDoador()
    {
        var doador = new Doador();

        doador.Nome = LeitorConsole.LerTexto("Nome completo: ");
        doador.Cpf = LeitorConsole.LerTexto("CPF (somente números ou com pontuação): ");
        doador.DataNascimento = LeitorConsole.LerData("Data de nascimento (dd/MM/yyyy): ");

        doador.TipoSanguineo = LeitorConsole.LerTexto("Tipo sanguíneo (A, B, AB, O): ").ToUpper();
        doador.FatorRh = LeitorConsole.LerTexto("Fator Rh (+ ou -): ");
        doador.Sexo = LeitorConsole.LerTexto("Sexo (M ou F): ").ToUpper();

        doador.Peso = LeitorConsole.LerDecimal("Peso em kg (ex: 65,5): ");
        doador.Telefone = LeitorConsole.LerTextoOpcional("Telefone (opcional): ");
        doador.Email = LeitorConsole.LerTextoOpcional("E-mail (opcional): ");
        doador.Endereco = LeitorConsole.LerTextoOpcional("Endereço (opcional): ");

        doador.Ativo = true;

        return doador;
    }

    public void LerDadosAtualizacao(Doador doador)
    {
        var nome = LeitorConsole.LerTextoOpcional($"Nome [{doador.Nome}]: ");
        if (nome != null) doador.Nome = nome;

        var cpf = LeitorConsole.LerTextoOpcional($"CPF [{doador.Cpf}]: ");
        if (cpf != null) doador.Cpf = Validadores.FormatarCpf(cpf);

        var dataNascStr = LeitorConsole.LerTextoOpcional($"Data de nascimento [{doador.DataNascimento:dd/MM/yyyy}]: ");
        if (dataNascStr != null)
        {
            if (DateTime.TryParseExact(dataNascStr, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dataNasc))
            {
                doador.DataNascimento = dataNasc;
            }
            else
            {
                LeitorConsole.ExibirErro("Data inválida, mantendo o valor atual.");
            }
        }

        var tipo = LeitorConsole.LerTextoOpcional($"Tipo sanguíneo [{doador.TipoSanguineo}]: ");
        if (tipo != null) doador.TipoSanguineo = tipo.ToUpper();

        var fator = LeitorConsole.LerTextoOpcional($"Fator Rh [{doador.FatorRh}]: ");
        if (fator != null) doador.FatorRh = fator;

        var sexo = LeitorConsole.LerTextoOpcional($"Sexo [{doador.Sexo}]: ");
        if (sexo != null) doador.Sexo = sexo.ToUpper();

        var pesoStr = LeitorConsole.LerTextoOpcional($"Peso [{doador.Peso:F1}]: ");
        if (pesoStr != null)
        {
            if (decimal.TryParse(pesoStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.GetCultureInfo("pt-BR"), out var peso))
            {
                doador.Peso = peso;
            }
            else if (decimal.TryParse(pesoStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out peso))
            {
                doador.Peso = peso;
            }
            else
            {
                LeitorConsole.ExibirErro("Peso inválido, mantendo o valor atual.");
            }
        }

        var telefone = LeitorConsole.LerTextoOpcional($"Telefone [{doador.Telefone}]: ");
        if (telefone != null) doador.Telefone = telefone;

        var email = LeitorConsole.LerTextoOpcional($"E-mail [{doador.Email ?? ""}]: ");
        if (email != null) doador.Email = email;

        var endereco = LeitorConsole.LerTextoOpcional($"Endereço [{doador.Endereco ?? ""}]: ");
        if (endereco != null) doador.Endereco = endereco;
    }

    public int LerId(string prompt = "ID do doador: ") => LeitorConsole.LerInt(prompt);

    public string LerTexto(string prompt) => LeitorConsole.LerTexto(prompt);

    public bool LerConfirmacao(string prompt) => LeitorConsole.LerConfirmacao(prompt);

    public void ExibirDetalhes(Doador doador) => TelaRelatorios.ExibirDetalhesDoador(doador);

    public void ExibirTabela(List<Doador> doadores) => TelaRelatorios.ExibirTabelaDoadores(doadores);

    public void ExibirSucesso(string mensagem) => LeitorConsole.ExibirSucesso(mensagem);

    public void ExibirErro(string mensagem) => LeitorConsole.ExibirErro(mensagem);

    public void ExibirInfo(string mensagem) => LeitorConsole.ExibirInfo(mensagem);

    public void ExibirErrosValidacao(IEnumerable<string> erros)
    {
        Console.WriteLine();
        LeitorConsole.ExibirErro("Erros de validação encontrados:");
        foreach (var erro in erros)
            LeitorConsole.ExibirErro($"  • {erro}");
    }

    public void AguardarEnter() => LeitorConsole.AguardarEnter();
}
