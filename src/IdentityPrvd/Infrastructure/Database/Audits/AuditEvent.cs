﻿namespace IdentityPrvd.Infrastructure.Database.Audits;

public static class AuditEvent
{
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string SoftDelete = nameof(SoftDelete);
    public const string Delete = nameof(Delete);
}
