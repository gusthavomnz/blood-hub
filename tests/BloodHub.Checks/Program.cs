using BloodHub.Checks;

var resultado = ValidadoresChecks.Executar();
if (resultado != 0)
    return resultado;

if (args.Contains("--integration"))
    return await RepositorioChecks.ExecutarAsync();

return 0;
