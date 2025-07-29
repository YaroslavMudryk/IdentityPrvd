namespace IdentityPrvd.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ToStartDateTime(this DateOnly dateOnly) => dateOnly.ToDateTime(TimeOnly.MinValue);
    public static DateTime ToEndDateTime(this DateOnly dateOnly) => dateOnly.ToDateTime(TimeOnly.MaxValue);
}
