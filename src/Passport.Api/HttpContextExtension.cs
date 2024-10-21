using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Passport.Api
{
    public static class HttpContextExtension
    {
        public static bool TryParsePassportId(this HttpContext httpContext, out Guid guPassportId)
        {
            guPassportId = Guid.Empty;

            if (httpContext.User is null)
                return false;

            string? sPassportId = httpContext.User.FindFirstValue(PassportClaim.Id);

            if (Guid.TryParse(sPassportId, out guPassportId) == false)
                return false;

            return true;
        }
    }
}
