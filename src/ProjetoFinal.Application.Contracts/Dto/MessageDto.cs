using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;

namespace ProjetoFinal.Application.Contracts.Dto;

public class MessageDto
{
    public int Status { get; set; }
    public IList<string> Messages { get; set; } = new List<string>();

    public MessageDto()
    {
        Messages = new List<string>();
    }

    public MessageDto(BusinessException businessException)
    {
        Status = (int)businessException.Status;
        Messages = businessException.Mensagens ?? new List<string>();
    }

    public MessageDto(Exception exception)
    {
        Status = ECodigoValue.ErroInterno;
        GetErros(exception);
    }

    public MessageDto(int status, params string[] messages)
    {
        Status = status;
        Messages = messages;
    }

    public MessageDto(ECodigo status, params string[] messages)
    {
        Status = (int)status;
        Messages = messages;
    }

    private void GetErros(Exception exception)
    {
        if (exception.InnerException != null)
        {
            GetErros(exception.InnerException);
        }

        Messages.Add(exception.Message);
    }
}