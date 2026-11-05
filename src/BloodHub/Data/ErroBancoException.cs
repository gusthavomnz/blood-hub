namespace BloodHub.Data;

public sealed class ErroBancoException(string mensagem, Exception causa)
    : Exception(mensagem, causa);
