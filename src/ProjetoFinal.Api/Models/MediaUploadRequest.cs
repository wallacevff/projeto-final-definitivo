using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Api.Models;

public class MediaUploadRequest
{
    [Required]
    public IFormFile? File { get; set; }

    public MediaKind? Kind { get; set; }
}
