using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Tests.IntegrationInfra;

public class TestIdentityPrvdContext(DbContextOptions<IdentityPrvdContext> options) : IdentityPrvdContext(options)
{
    protected override void SaveAuditsItems()
    {

    }

    protected override void OnBeforeSaveChanges()
    {

    }
}
