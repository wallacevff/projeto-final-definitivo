using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using ProjetoFinal.Api.Utils;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;

namespace ProjetoFinal.Api.Middlewares;

public class ExceptionHandlingMidleware(RequestDelegate next, ILogger<ExceptionHandlingMidleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMidleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is BusinessException businessException)
        {
            _logger.LogError(exception, "Ocorreu um erro na requisição");
            // context.Response.Headers["Status-Description"] = EcodigoUtils.GetDescription(businessException.Status);
            context.Response.HttpContext.Features.Get<IHttpResponseFeature>()!.ReasonPhrase = EcodigoUtils
                .GetDescription(businessException.Status);
            context.Response.StatusCode = (short)businessException.Status;
            if (businessException.Mensagens is null)
            {
                var response =
                    new MessageDto(businessException.Status, businessException.Message);
                await context.Response.WriteAsJsonAsync(response, options: new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = new PascalCaseNamingPolicy()
                });
                return;
            }
            else
            {
                var response =
                    new MessageDto(businessException);
                await context.Response.WriteAsJsonAsync(response, options: new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = new PascalCaseNamingPolicy()
                });
                return;
            }
        }

        _logger.LogError(exception, "Ocorreu um erro interno");
        context.Response.StatusCode = ECodigoValue.ErroInterno;
        var responseEx = new MessageDto(exception);
        await context.Response.WriteAsJsonAsync(responseEx, options: new JsonSerializerOptions()
        {
            PropertyNamingPolicy = new PascalCaseNamingPolicy()
        });
    }
}