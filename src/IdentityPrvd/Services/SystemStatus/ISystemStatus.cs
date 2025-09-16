namespace IdentityPrvd.Services.SystemStatus;

public interface ISystemStatus
{
    Task<SystemStatus> GetSystemStatusAsync();
}
