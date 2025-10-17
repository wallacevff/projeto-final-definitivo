namespace ProjetoFinal.Domain.Shared.Enums;

public enum ECodigo
{
    Sucesso = 200,
    MaRequisicao = 400,
    NaoEncontrado = 404,
    NaoAutenticado = 401,
    NaoPermitido = 403,
    Conflito = 409,
    ErroInterno = 500,
}

public abstract class ECodigoValue
{
    public const int Sucesso = 200;
    public const int MaRequisicao = 400;
    public const int NaoEncontrado = 404;
    public const int NaoAutenticado = 401;
    public const int NaoPermitido = 403;
    public const int Conflito = 409;
    public const int ErroInterno = 500;
}

public static class EcodigoUtils
{
    public static string GetDescription(ECodigo status)
    {
        return status switch
        {
            ECodigo.NaoEncontrado => "Recurso nao encontrado.",
            ECodigo.NaoAutenticado => "Autenticacao necessaria.",
            ECodigo.NaoPermitido => "Acesso nao permitido.",
            ECodigo.Conflito => "Conflito na solicitacao.",
            ECodigo.ErroInterno => "Erro interno do servidor.",
            _ => "Status desconhecido."
        };
    }
}