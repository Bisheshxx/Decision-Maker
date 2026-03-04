using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Response;

namespace DecisionMaker.Helpers;

public static class CookieHelper
{
    public static void SetAuthCookies(HttpResponse response, string access_token, string refresh_token, bool is_production = false)
    {
        var access_option = new CookieOptions
        {
            HttpOnly = true,
            Secure = is_production,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddMinutes(15)
        };
        var refresh_options = new CookieOptions
        {
            HttpOnly = true,
            Secure = is_production,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddHours(7)
        };
        response.Cookies.Append("access_token", access_token, access_option);
        response.Cookies.Append("refresh_token", refresh_token, refresh_options);
    }
}
