using ProjetoFinal.Domain.Shared.Enums;

namespace ProjetoFinal.Domain.Shared.Exceptions;

public class BusinessException(string mensagem, ECodigo status, IList<string>? mensagens = null) : Exception(mensagem)
{
    public ECodigo Status { get; private set; } = status;
    public IList<string>? Mensagens { get; private set; } = mensagens;
}