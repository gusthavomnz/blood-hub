using System.ComponentModel.DataAnnotations;
using BloodHub.Modelos;
using BloodHub.Repositorios;
using BloodHub.Telas;
using BloodHub.Validacao;
using MySqlConnector;

namespace BloodHub.Controladores;

public sealed class DoadorController(IDoadorRepositorio repositorio, TelaDoador tela)
{
    private readonly IDoadorRepositorio _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    private readonly TelaDoador _tela = tela ?? throw new ArgumentNullException(nameof(tela));

    public async Task ExecutarMenuAsync()
    {
        while (true)
        {
            var opcao = _tela.ExibirMenu();

            try
            {
                switch (opcao)
                {
                    case 1: await CadastrarAsync(); break;
                    case 2: await ListarAsync(); break;
                    case 3: await BuscarAsync(); break;
                    case 4: await AtualizarAsync(); break;
                    case 5: await ExcluirAsync(); break;
                    case 6: return;
                }
            }
            catch (MySqlException ex)
            {
                var msg = TradutorErroBanco.Traduzir(ex);
                _tela.ExibirErro(msg ?? $"Erro de banco de dados: {ex.Message}");
            }
            catch (ValidationException ex)
            {
                _tela.ExibirErro(ex.Message);
            }
            catch (Exception ex)
            {
                _tela.ExibirErro($"Erro inesperado: {ex.Message}");
            }

            _tela.AguardarEnter();
        }
    }

    private async Task CadastrarAsync()
    {
        _tela.ExibirCabecalhoCadastro();

        var doador = _tela.LerDadosDoador();

        var erros = Validadores.ValidarDoador(doador);
        if (erros.Count > 0)
        {
            _tela.ExibirErrosValidacao(erros);
            return;
        }

        doador.Cpf = Validadores.FormatarCpf(doador.Cpf);

        try
        {
            var id = await _repositorio.InserirAsync(doador);
            _tela.ExibirSucesso($"Doador cadastrado com sucesso! ID: {id}");
        }
        catch (MySqlException ex)
        {
            var msg = TradutorErroBanco.Traduzir(ex);
            _tela.ExibirErro(msg ?? $"Erro ao cadastrar: {ex.Message}");
        }
    }

    private async Task ListarAsync()
    {
        var doadores = await _repositorio.ListarTodosAsync();
        _tela.ExibirTabela(doadores);
    }

    private async Task BuscarAsync()
    {
        var opcao = _tela.ExibirMenuBusca();

        switch (opcao)
        {
            case 1:
                await BuscarPorIdAsync();
                break;
            case 2:
                await BuscarPorCpfAsync();
                break;
            case 3:
                await BuscarPorNomeAsync();
                break;
            case 4:
                return;
        }
    }

    private async Task BuscarPorIdAsync()
    {
        var id = _tela.LerId("ID do doador: ");
        var doador = await _repositorio.BuscarPorIdAsync(id);

        if (doador == null)
        {
            _tela.ExibirErro($"Doador com ID {id} não encontrado.");
            return;
        }

        _tela.ExibirDetalhes(doador);
    }

    private async Task BuscarPorCpfAsync()
    {
        var cpfBruto = _tela.LerTexto("CPF do doador: ");
        var cpf = Validadores.FormatarCpf(cpfBruto);

        var doador = await _repositorio.BuscarPorCpfAsync(cpf);

        if (doador == null)
        {
            _tela.ExibirErro($"Doador com CPF {cpf} não encontrado.");
            return;
        }

        _tela.ExibirDetalhes(doador);
    }

    private async Task BuscarPorNomeAsync()
    {
        var nome = _tela.LerTexto("Nome (ou parte do nome): ");
        var doadores = await _repositorio.BuscarPorNomeAsync(nome);

        if (doadores.Count == 0)
        {
            _tela.ExibirErro($"Nenhum doador encontrado com o nome '{nome}'.");
            return;
        }

        _tela.ExibirTabela(doadores);
    }

    private async Task AtualizarAsync()
    {
        var id = _tela.LerId("ID do doador a atualizar: ");
        var doador = await _repositorio.BuscarPorIdAsync(id);

        if (doador == null)
        {
            _tela.ExibirErro($"Doador com ID {id} não encontrado.");
            return;
        }

        _tela.ExibirDetalhes(doador);
        _tela.ExibirAvisoAtualizacao();

        _tela.LerDadosAtualizacao(doador);

        var erros = Validadores.ValidarDoador(doador);
        if (erros.Count > 0)
        {
            _tela.ExibirErrosValidacao(erros);
            return;
        }

        if (!_tela.LerConfirmacao("Confirma a atualização?"))
        {
            _tela.ExibirInfo("Atualização cancelada.");
            return;
        }

        try
        {
            var sucesso = await _repositorio.AtualizarAsync(doador);
            if (sucesso)
                _tela.ExibirSucesso("Doador atualizado com sucesso!");
            else
                _tela.ExibirErro("Nenhuma alteração foi realizada.");
        }
        catch (MySqlException ex)
        {
            var msg = TradutorErroBanco.Traduzir(ex);
            _tela.ExibirErro(msg ?? $"Erro ao atualizar: {ex.Message}");
        }
    }

    private async Task ExcluirAsync()
    {
        var id = _tela.LerId("ID do doador a excluir: ");
        var doador = await _repositorio.BuscarPorIdAsync(id);

        if (doador == null)
        {
            _tela.ExibirErro($"Doador com ID {id} não encontrado.");
            return;
        }

        _tela.ExibirDetalhes(doador);

        if (!_tela.LerConfirmacao("Tem certeza que deseja EXCLUIR este doador?"))
        {
            _tela.ExibirInfo("Operação cancelada.");
            return;
        }

        try
        {
            await _repositorio.ExcluirAsync(id);
            _tela.ExibirSucesso("Doador excluído com sucesso!");
        }
        catch (MySqlException ex) when (TradutorErroBanco.EhRestricaoIntegridade(ex))
        {
            var msg = TradutorErroBanco.Traduzir(ex);
            _tela.ExibirErro(msg ?? "Não é possível excluir este doador pois ele possui doações vinculadas.");

            if (_tela.LerConfirmacao("Deseja INATIVAR o doador em vez de excluí-lo?"))
            {
                try
                {
                    await _repositorio.InativarAsync(id);
                    _tela.ExibirSucesso("Doador inativado com sucesso!");
                }
                catch (MySqlException exInativar)
                {
                    var msgInativar = TradutorErroBanco.Traduzir(exInativar);
                    _tela.ExibirErro(msgInativar ?? $"Erro ao inativar: {exInativar.Message}");
                }
            }
            else
            {
                _tela.ExibirInfo("Operação cancelada.");
            }
        }
    }
}
