using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IdentityPrvd.WebApi.Db.Audits;

public class AuditBuilder
{
    private EntityState _state;
    private readonly Entities.Audit _audit = new();
    private bool _softDeleted;
    public static AuditBuilder NewDefaultAudit() => new();

    public AuditBuilder On(EntityState state)
    {
        _state = state;

        _audit.Event = GetEventType();

        return this;
    }

    public AuditBuilder SoftDeleted(bool softDeleted)
    {
        _softDeleted = softDeleted;
        return this;
    }

    public AuditBuilder With(Action<Entities.Audit> action)
    {
        action(_audit);
        return this;
    }

    public AuditBuilder WithChanges(EntityEntry entityEntry)
    {
        _audit.Changes = EntityParser.ParseChanges(entityEntry);
        return this;
    }

    public Entities.Audit Build() => _audit;

    private string GetEventType()
    {
        if (_state == EntityState.Deleted)
            return AuditEvent.Delete;
        if (_state == EntityState.Modified && _softDeleted)
            return AuditEvent.SoftDelete;
        if (_state == EntityState.Modified)
            return AuditEvent.Update;

        return AuditEvent.Create;
    }
}
