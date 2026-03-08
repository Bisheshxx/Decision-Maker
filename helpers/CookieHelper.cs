using DecisionMaker.Dtos.Account;
using DecisionMaker.Dtos.Response;

namespace DecisionMaker.Helpers;

public static class CookieHelper
{
    public static void SetAuthCookies(HttpResponse response, string access_token, string refresh_token, bool is_production = false)
    {
        SetAccessTokenCookie(response, access_token, is_production);
        SetRefreshTokenCookie(response, refresh_token, is_production);
    }

    public static void SetAccessTokenCookie(HttpResponse response, string access_token, bool is_production = false)
    {
        var access_option = new CookieOptions
        {
            HttpOnly = true,
            Secure = is_production,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddMinutes(15)
        };
        response.Cookies.Append("access_token", access_token, access_option);
    }

    public static void RemoveAccessTokenCookie(HttpResponse response, bool is_production = false)
    {
        var access_option = new CookieOptions
        {
            HttpOnly = true,
            Secure = is_production,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        response.Cookies.Append("access_token", "", access_option);
    }
    public static void RemoveRefreshTokenCookie(HttpResponse response, bool is_production = false)
    {
        var access_option = new CookieOptions
        {
            HttpOnly = true,
            Secure = is_production,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        response.Cookies.Append("refresh_token", "", access_option);
    }

    public static void SetRefreshTokenCookie(HttpResponse response, string refresh_token, bool is_production = false)
    {
        var refresh_options = new CookieOptions
        {
            HttpOnly = true,
            Secure = is_production,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(7)
        };
        response.Cookies.Append("refresh_token", refresh_token, refresh_options);
    }
    public static void RemoveAuthCookies(HttpResponse response)
    {
        RemoveAccessTokenCookie(response);
        RemoveRefreshTokenCookie(response);
    }
}
