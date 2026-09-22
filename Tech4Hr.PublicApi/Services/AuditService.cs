using Tech4Hr.PublicApi.Data;
using Tech4Hr.PublicApi.Models;

namespace Tech4Hr.PublicApi.Services;

public interface IAuditService
{
    void Add(Guid? userId, string action, string entityType, object? entityId = null);
}

public sealed class AuditService(PublicApiDbContext db) : IAuditService
{
    public void Add(Guid? userId, string action, string entityType, object? entityId = null)
    {
        db.AuditEvents.Add(new AuditEvent
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId?.ToString()
        });
    }
}
