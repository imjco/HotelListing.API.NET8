using HotelListingAPI.Configurations;
using HotelListingAPI.Core.Contracts;
using HotelListingAPI.Data;

using Microsoft.EntityFrameworkCore;
using Serilog;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotelListingAPI.Core.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using HotelListingAPI.Core.Repository;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System.Text.Json;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options => {
    options.UseSqlServer(connectionString);
    });


builder.Services.AddIdentityCore<ApiUser>()
    .AddRoles<IdentityRole>()
    .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelListingApi")
    .AddEntityFrameworkStores<HotelListingDbContext>()
    .AddDefaultTokenProviders();
 
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel Listing API", Version = "v1" });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 1234abcdefg",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                },
                Scheme = "0auth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

});

builder.Services.AddCors(option => { 
    option.AddPolicy(
        "AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
        );
});

builder.Services.AddApiVersioning(option =>
{
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.DefaultApiVersion = new ApiVersion(1, 0);
    option.ReportApiVersions = true;
    option.ApiVersionReader = ApiVersionReader.Combine(
               new HeaderApiVersionReader("X-Version"),
               new QueryStringApiVersionReader("ver")
    );
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});






builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddAutoMapper(typeof(MapperConfig));


builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IHotelsRepository, HotelRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

builder.Services.AddAuthentication(options => { 
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});

builder.Services.AddResponseCaching(options => { 
    
    options.MaximumBodySize = 1024;
    options.UseCaseSensitivePaths = true;
});

builder.Services.AddHealthChecks()
    .AddTypeActivatedCheck<CustomHealthCheck>("custom",
        failureStatus: HealthStatus.Degraded, 
        tags: new[] {"custom" })
    .AddSqlServer(connectionString, tags: new[] { "database" })
    .AddDbContextCheck<HotelListingDbContext>(tags: new[] { "database" });

builder.Services.AddControllers().AddOData(options =>
{
    options.Select().Filter().OrderBy().Expand().Count();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{ 
app.UseSwagger();

app.UseSwaggerUI();

//}

app.MapHealthChecks("/healthcheck", new HealthCheckOptions {
    Predicate = heatlhcheck => heatlhcheck.Tags.Contains("custom"),
    ResultStatusCodes =
    {
         [HealthStatus.Healthy] = StatusCodes.Status200OK,
         [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
         [HealthStatus.Degraded] = StatusCodes.Status200OK,
    },
          
    ResponseWriter = WriteResponse
});

app.MapHealthChecks("/databasehealthcheck", new HealthCheckOptions {
    Predicate = heatlhcheck => heatlhcheck.Tags.Contains("database"),
    ResultStatusCodes =
    {
         [HealthStatus.Healthy] = StatusCodes.Status200OK,
         [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
         [HealthStatus.Degraded] = StatusCodes.Status200OK,
    },
          
    ResponseWriter = WriteResponse
});

app.MapHealthChecks("/health");

static Task WriteResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var options = new JsonWriterOptions { Indented = true };

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("status", healthReport.Status.ToString());
        jsonWriter.WriteStartObject("results");

        foreach (var healthReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject(healthReportEntry.Key);
            jsonWriter.WriteString("status",
                healthReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("description",
                healthReportEntry.Value.Description);
            jsonWriter.WriteStartObject("data");

            foreach (var item in healthReportEntry.Value.Data)
            {
                jsonWriter.WritePropertyName(item.Key);
                System.Text.Json.JsonSerializer.Serialize(jsonWriter, item.Value,
                item.Value?.GetType() ?? typeof(object));
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }

        return context.Response.WriteAsync(
        Encoding.UTF8.GetString(memoryStream.ToArray()));

    }
}




app.UseMiddleware<ExceptionMiddleware>();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseResponseCaching();

app.Use(async (context, next) =>
{
    context.Response.GetTypedHeaders().CacheControl = 
    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
    {
        Public = true,
        MaxAge = TimeSpan.FromSeconds(2)
    };
    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
    new string[] { "Accept-Encoding" };
    await next();
});


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



public class CustomHealthCheck : IHealthCheck
{

    private readonly int _arg1;
    private readonly string _arg2;

    //public SampleHealthCheckWithArgs(int arg1, string arg2)
    //{
    //    (_arg1, _arg2) = (arg1, arg2);
    //}
       
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var isHealthy = true;

        if(isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("All systems are looking good!"));
        }
        else
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, "System Unhealthy"));
        }
    }
}