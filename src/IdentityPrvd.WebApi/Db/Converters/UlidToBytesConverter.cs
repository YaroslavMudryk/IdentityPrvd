using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IdentityPrvd.WebApi.Db.Converters;

public class UlidToBytesConverter(ConverterMappingHints mappingHints) : ValueConverter<Ulid, byte[]>(
            convertToProviderExpression: x => x.ToByteArray(),
            convertFromProviderExpression: x => new Ulid(x),
            mappingHints: DefaultHints.With(mappingHints))
{
    private static readonly ConverterMappingHints DefaultHints = new ConverterMappingHints(size: 16);

    public UlidToBytesConverter() : this(null)
    {

    }
}
