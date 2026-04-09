using DecisionMaker.Data;
using DecisionMaker.Dtos.Response;
using DecisionMaker.Helpers;
using DecisionMaker.Interfaces;
using DecisionMaker.Interfaces.Auth;
using DecisionMaker.Interfaces.Decision;
using DecisionMaker.Middleware;
using DecisionMaker.Models;
using DecisionMaker.Services;
using DecisionMaker.Services.Auth;
using DecisionMaker.Services.DecisionService;
using DecisionMaker.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var isProduction = builder.Environment.IsProduction();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            var response = ApiResponse<object>.Fail("Unauthorized - Invalid or missing token", ErrorType.Unauthorized);
            return context.Response.WriteAsJsonAsync(response);
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            var response = ApiResponse<object>.Fail("Forbidden", ErrorType.Forbidden);
            return context.Response.WriteAsJsonAsync(response);
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<ITokenService, TokenService>();
var frontendURL = builder.Configuration["AppSettings:FrontendUrl"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = async context =>
            {

                var token = context.Request.Cookies["access_token"];
                var refresh_token = context.Request.Cookies["refresh_token"];


                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                    // Console.WriteLine($"Token received from cookie: {token.Substring(0, Math.Min(20, token.Length))}...");
                }
                else if (!context.Request.Path.StartsWithSegments("/api/accounts/Refresh"))
                {
                    var refreshToken = context.Request.Cookies["refresh_token"];
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                        var refreshResult = await authService.RefreshAsync(refreshToken);

                        if (refreshResult.Success && refreshResult.Data != null)
                        {

                            CookieHelper.SetAccessTokenCookie(context.Response, refreshResult.Data.Token!, isProduction);
                            CookieHelper.SetRefreshTokenCookie(context.Response, refreshResult.Data.RefreshToken!, isProduction);
                            context.Token = refreshResult.Data.Token;
                        }
                        else
                        {
                            Console.WriteLine("Token refresh failed in OnMessageReceived");
                        }
                    }
                }
            },
            OnAuthenticationFailed = async context =>
            {
                var refreshToken = context.Request.Cookies["refresh_token"];
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();

                    var refreshResult = await authService.RefreshAsync(refreshToken);

                    if (refreshResult.Success && refreshResult.Data != null)
                    {
                        // Console.WriteLine("Token refreshed successfully, setting new cookies");
                        CookieHelper.SetAccessTokenCookie(context.Response, refreshResult.Data.Token!, isProduction);
                        CookieHelper.SetRefreshTokenCookie(context.Response, refreshResult.Data.RefreshToken!, isProduction);
                        context.NoResult();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var response = ApiResponse<object>.Fail("Token expired, refreshed - please retry", ErrorType.Unauthorized);
                        await context.Response.WriteAsJsonAsync(response);
                        return;
                    }
                }
                context.NoResult();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var unauthorizedResponse = ApiResponse<object>.Fail($"Unauthorized: {context.Exception.Message}", ErrorType.Unauthorized);
                await context.Response.WriteAsJsonAsync(unauthorizedResponse);
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var response = ApiResponse<object>.Fail("Unauthorized - Invalid or missing token", ErrorType.Unauthorized);
                return context.Response.WriteAsJsonAsync(response);
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                var response = ApiResponse<object>.Fail("Forbidden", ErrorType.Forbidden);
                return context.Response.WriteAsJsonAsync(response);
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],

            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }).AddGoogle(option =>
    {
        option.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        option.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        option.Scope.Add("profile");
        option.ClaimActions.MapJsonKey("picture", "picture");
    }).AddCookie();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddScoped<IAuthService, AuthServices>();
builder.Services.AddScoped<IDecisionService, DecisionServices>();
// builder.Services.AddScoped<IDecisionItemService, DecisionItemService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(frontendURL!)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Authentication for Decision Maker",
        Version = "v1",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "access_token",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Description = "Authentication via HttpOnly cookie"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()

        }
    });

});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var response = ApiResponse<object>.Fail(
            errors,
            ErrorType.Validation,
            "Validation failed"
        );

        return new BadRequestObjectResult(response);
    };
});


var app = builder.Build();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = ApiResponse<object>.Fail("Internal server error", ErrorType.ServerError);
        await context.Response.WriteAsJsonAsync(response);
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");
app.UseMiddleware<ExceptionMiddleware>();

// Only use HTTPS redirection in production since we're running on HTTP in development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", (HttpContext context) =>
{
    var utcNow = DateTimeOffset.UtcNow;
    var requestedTimeZone = context.Request.Headers["X-Timezone"].FirstOrDefault();

    var timeZone = TimeZoneInfo.Utc;
    var timeZoneSource = "utc-fallback";

    if (!string.IsNullOrWhiteSpace(requestedTimeZone))
    {
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(requestedTimeZone);
            timeZoneSource = "request-header";
        }
        catch (TimeZoneNotFoundException)
        {
            timeZoneSource = "invalid-request-header";
        }
        catch (InvalidTimeZoneException)
        {
            timeZoneSource = "invalid-request-header";
        }
    }

    var localNow = TimeZoneInfo.ConvertTime(utcNow, timeZone);
    var region =
        context.Request.Headers["X-Azure-Region"].FirstOrDefault()
        ?? Environment.GetEnvironmentVariable("WEBSITE_REGION")
        ?? "unknown";

    return Results.Ok(new
    {
        status = "API is running",
        utcNow,
        localNow,
        timeZone = timeZone.Id,
        region,
        timeZoneSource
    });
}).AllowAnonymous();
app.MapControllers();

app.Run();
