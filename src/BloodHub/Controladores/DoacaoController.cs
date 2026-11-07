using System.ComponentModel.DataAnnotations;
using BloodHub.Modelos;
using BloodHub.Repositorios;

namespace BloodHub.Controladores;

/// <summary>
/// Controlador responsável por orquestrar o fluxo de dados, validações e persistência de Doações.
/// Segue a arquitetura MVC pura, sem interação direta com o console (I/O).
/// </summary>
public sealed class DoacaoController(IDoacaoRepositorio doacaoRepositorio, IDoadorRepositorio doadorRepositorio)
{
    private readonly IDoacaoRepositorio _doacaoRepositorio = doacaoRepositorio ?? throw new ArgumentNullException(nameof(doacaoRepositorio));
    private readonly IDoadorRepositorio _doadorRepositorio = doadorRepositorio ?? throw new ArgumentNullException(nameof(doadorRepositorio));

    /// <summary>
    /// Registra uma nova doação no sistema após validar a existência e atividade do doador.
    /// </summary>
    public async Task<Doacao> RegistrarAsync(Doacao doacao)
    {
        ArgumentNullException.ThrowIfNull(doacao);
        ValidarDadosDoacao(doacao);

        var doador = await _doadorRepositorio.BuscarPorIdAsync(doacao.DoadorId)
            ?? throw new ValidationException($"Doador com ID {doacao.DoadorId} não encontrado.");

        if (!doador.Ativo)
            throw new ValidationException("Não é permitido registrar doações para um doador inativo.");

        // Normalização de dados
        doacao.LocalColeta = string.IsNullOrWhiteSpace(doacao.LocalColeta) ? "Hemocentro Central" : doacao.LocalColeta.Trim();
        doacao.Status = string.IsNullOrWhiteSpace(doacao.Status) ? "Realizada" : doacao.Status.Trim();
        doacao.MotivoRecusa = string.IsNullOrWhiteSpace(doacao.MotivoRecusa) ? null : doacao.MotivoRecusa.Trim();
        doacao.Observacao = string.IsNullOrWhiteSpace(doacao.Observacao) ? null : doacao.Observacao.Trim();

        await _doacaoRepositorio.InserirAsync(doacao);
        doacao.Doador = doador;
        return doacao;
    }

    /// <summary>
    /// Retorna todas as doações com os dados completos dos doadores associados (JOIN).
    /// </summary>
    public Task<List<Doacao>> ListarTodasComDoadorAsync() => _doacaoRepositorio.ListarComDoadorAsync();

    /// <summary>
    /// Busca uma doação específica pelo seu ID.
    /// </summary>
    public Task<Doacao?> BuscarPorIdAsync(int id)
    {
        ValidarId(id);
        return _doacaoRepositorio.BuscarPorIdAsync(id);
    }

    /// <summary>
    /// Lista o histórico de todas as doações de um doador específico.
    /// </summary>
    public async Task<List<Doacao>> ListarPorDoadorAsync(int doadorId)
    {
        ValidarId(doadorId, "O ID do doador deve ser positivo.");
        var doador = await _doadorRepositorio.BuscarPorIdAsync(doadorId)
            ?? throw new ValidationException($"Doador com ID {doadorId} não encontrado.");

        return await _doacaoRepositorio.ListarPorDoadorAsync(doadorId);
    }

    /// <summary>
    /// Lista o histórico exclusivo de doações 'Realizadas' de um doador (usado pelo motor de elegibilidade).
    /// </summary>
    public Task<List<Doacao>> ObterHistoricoRealizadasAsync(int doadorId)
    {
        ValidarId(doadorId, "O ID do doador deve ser positivo.");
        return _doacaoRepositorio.ListarRealizadasDoDoadorAsync(doadorId);
    }

    /// <summary>
    /// Lista doações realizadas dentro de um intervalo de datas com os doadores associados.
    /// </summary>
    public Task<List<Doacao>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        if (inicio.Date > fim.Date)
            throw new ValidationException("A data inicial não pode ser superior à data final.");

        return _doacaoRepositorio.ListarPorPeriodoAsync(inicio, fim);
    }

    /// <summary>
    /// Atualiza os dados de uma doação existente.
    /// </summary>
    public async Task<bool> AtualizarAsync(Doacao doacao)
    {
        ArgumentNullException.ThrowIfNull(doacao);
        ValidarId(doacao.Id);
        ValidarDadosDoacao(doacao);

        var existente = await _doacaoRepositorio.BuscarPorIdAsync(doacao.Id)
            ?? throw new ValidationException($"Doação com ID {doacao.Id} não encontrada para atualização.");

        var doador = await _doadorRepositorio.BuscarPorIdAsync(doacao.DoadorId)
            ?? throw new ValidationException($"Doador com ID {doacao.DoadorId} não encontrado.");

        return await _doacaoRepositorio.AtualizarAsync(doacao);
    }

    /// <summary>
    /// Remove uma doação do sistema pelo seu identificador.
    /// </summary>
    public async Task<bool> ExcluirAsync(int id)
    {
        ValidarId(id);
        var existente = await _doacaoRepositorio.BuscarPorIdAsync(id)
            ?? throw new ValidationException($"Doação com ID {id} não encontrada para exclusão.");

        return await _doacaoRepositorio.ExcluirAsync(id);
    }

    private static void ValidarDadosDoacao(Doacao doacao)
    {
        ValidarId(doacao.DoadorId, "O ID do doador informado é inválido.");

        if (doacao.DataDoacao == default)
            throw new ValidationException("A data da doação é obrigatória.");

        if (doacao.DataDoacao.Date > DateTime.Today)
            throw new ValidationException("A data da doação não pode ser uma data futura.");

        if (doacao.PesoKg <= 0 || doacao.PesoKg > 999.99m)
            throw new ValidationException("O peso registrado na triagem deve ser maior que zero e válido.");

        if (doacao.VolumeMl <= 0 || doacao.VolumeMl > 1000)
            throw new ValidationException("O volume coletado deve ser maior que zero e no máximo 1000 mL.");

        if (doacao.Status is not ("Realizada" or "Recusada"))
            throw new ValidationException("O status da doação deve ser 'Realizada' ou 'Recusada'.");

        if (doacao.Status == "Recusada" && string.IsNullOrWhiteSpace(doacao.MotivoRecusa))
            throw new ValidationException("O motivo da recusa é obrigatório quando o status for 'Recusada'.");
    }

    private static void ValidarId(int id, string mensagem = "O identificador deve ser um número inteiro positivo.")
    {
        if (id <= 0)
            throw new ValidationException(mensagem);
    }
}
