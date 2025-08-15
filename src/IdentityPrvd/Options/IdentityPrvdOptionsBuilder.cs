namespace IdentityPrvd.Options;

public class IdentityPrvdOptionsBuilder
{
    private IdentityPrvdOptions _options;

    public IdentityPrvdOptionsBuilder(IdentityPrvdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    public IdentityPrvdOptions Options
        => _options;
}
