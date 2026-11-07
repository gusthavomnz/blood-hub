using BloodHub.Data;
using BloodHub.Modelos;
using MySqlConnector;

namespace BloodHub.Repositorios;

/// <summary>
/// Repositório para consultas analíticas e relatórios agregados (GROUP BY) utilizando ADO.NET puro com MySqlConnector.
/// </summary>
public sealed class RelatorioRepositorio(ConnectionFactory factory) : IRelatorioRepositorio
{
    /// <summary>
    /// Agrupa as doações realizadas por tipo sanguíneo (ex: 'O+', 'A-'),
    /// calculando o total de doações e o volume total em mL.
    /// </summary>
    public async Task<List<ResumoPorTipoSanguineo>> TotalPorTipoSanguineoAsync()
    {
        const string sql = """
            SELECT 
                CONCAT(doad.tipo_sanguineo, doad.fator_rh) AS tipo_sanguineo,
                COUNT(d.id) AS total_doacoes,
                COALESCE(SUM(d.volume_ml), 0) AS volume_total_ml
            FROM doacoes d
            INNER JOIN doadores doad ON d.doador_id = doad.id
            WHERE d.status = 'Realizada'
            GROUP BY doad.tipo_sanguineo, doad.fator_rh
            ORDER BY total_doacoes DESC, tipo_sanguineo ASC;
            """;

        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        await using var leitor = await comando.ExecuteReaderAsync();

        var relatorio = new List<ResumoPorTipoSanguineo>();
        while (await leitor.ReadAsync())
        {
            relatorio.Add(new ResumoPorTipoSanguineo
            {
                TipoSanguineo = leitor.GetString("tipo_sanguineo"),
                TotalDoacoes = Convert.ToInt32(leitor["total_doacoes"]),
                VolumeTotalMl = Convert.ToInt64(leitor["volume_total_ml"])
            });
        }

        return relatorio;
    }

    /// <summary>
    /// Agrupa as doações realizadas por ano e mês,
    /// calculando a quantidade total de coletas e o volume total em mL.
    /// </summary>
    public async Task<List<DoacoesPorMes>> DoacoesPorMesAsync()
    {
        const string sql = """
            SELECT 
                YEAR(data_doacao) AS ano,
                MONTH(data_doacao) AS mes,
                COUNT(id) AS total_doacoes,
                COALESCE(SUM(volume_ml), 0) AS volume_total_ml
            FROM doacoes
            WHERE status = 'Realizada'
            GROUP BY YEAR(data_doacao), MONTH(data_doacao)
            ORDER BY ano DESC, mes DESC;
            """;

        await using var conexao = await factory.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        await using var leitor = await comando.ExecuteReaderAsync();

        var relatorio = new List<DoacoesPorMes>();
        while (await leitor.ReadAsync())
        {
            relatorio.Add(new DoacoesPorMes
            {
                Ano = Convert.ToInt32(leitor["ano"]),
                Mes = Convert.ToInt32(leitor["mes"]),
                TotalDoacoes = Convert.ToInt32(leitor["total_doacoes"]),
                VolumeTotalMl = Convert.ToInt64(leitor["volume_total_ml"])
            });
        }

        return relatorio;
    }
}
