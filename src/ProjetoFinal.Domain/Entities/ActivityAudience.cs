using System;

namespace ProjetoFinal.Domain.Entities;

public class ActivityAudience : AuditableEntity
{
    public Guid ActivityId { get; set; }
    public Guid ClassGroupId { get; set; }

    public Activity? Activity { get; set; }
    public ClassGroup? ClassGroup { get; set; }
}
