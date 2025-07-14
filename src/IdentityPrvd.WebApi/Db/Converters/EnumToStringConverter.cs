using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IdentityPrvd.WebApi.Db.Converters;

public class EnumToStringConverter(ConverterMappingHints mappingHints) : ValueConverter<Enum, string>(
            convertToProviderExpression: x => x.ToString(),
            convertFromProviderExpression: x => (Enum)Enum.Parse(x.GetType(), x),
            mappingHints: mappingHints)
{
    public EnumToStringConverter() : this(null)
    {

    }
}
