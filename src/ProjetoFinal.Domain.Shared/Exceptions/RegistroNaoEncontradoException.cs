using ProjetoFinal.Domain.Shared.Enums;

namespace ProjetoFinal.Domain.Shared.Exceptions;

public class RegistroNaoEncontradoException(
    string message,
    ECodigo codigo = ECodigo.NaoEncontrado,
    IList<string>? mensagens = null) : BusinessException(message, codigo, mensagens)
{
}