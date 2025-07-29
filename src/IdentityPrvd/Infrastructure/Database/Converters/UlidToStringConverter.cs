using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IdentityPrvd.Infrastructure.Database.Converters;

public class UlidToStringConverter(ConverterMappingHints mappingHints) : ValueConverter<Ulid, string>(
            convertToProviderExpression: x => x.ToString(),
            convertFromProviderExpression: x => Ulid.Parse(x),
            mappingHints: DefaultHints.With(mappingHints))
{
    private static readonly ConverterMappingHints DefaultHints = new ConverterMappingHints(size: 26);

    public UlidToStringConverter() : this(null)
    {

    }
}
