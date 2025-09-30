using System;

namespace ProjetoFinal.Domain.Entities;

public class ActivityAttachment : AuditableEntity
{
    public Guid ActivityId { get; set; }
    public Guid MediaResourceId { get; set; }
    public string? Caption { get; set; }

    public Activity? Activity { get; set; }
    public MediaResource? MediaResource { get; set; }
}
