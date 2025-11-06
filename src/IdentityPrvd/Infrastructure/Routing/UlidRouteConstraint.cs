using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Infrastructure.Routing;

public class UlidRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var value) || value == null)
        {
            return false;
        }

        var valueString = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
        
        if (string.IsNullOrWhiteSpace(valueString))
        {
            return false;
        }

        return Ulid.TryParse(valueString, out _);
    }
}

