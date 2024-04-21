using Microsoft.EntityFrameworkCore;
using TokenTest.Services;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.HttpOverrides;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/logs.txt")
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"])),
            NameClaimType = ClaimTypes.Name
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                Log.Logger.Information($"Received JWT: {token}");
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                Log.Logger.Error($"Authentication failed: {context.Exception}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Log.Logger.Warning($"Authorization challenge: {context.AuthenticateFailure} {context.Error} {context.ErrorDescription} {context.ErrorUri}");
                return Task.CompletedTask;
            }
        };
    });

var connection = String.Empty;
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddEnvironmentVariables().AddJsonFile("appsettings.Development.json");
    connection = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
}
else
{
    builder.Configuration.AddEnvironmentVariables().AddJsonFile("appsettings.json");
    connection = builder.Configuration.GetConnectionString("SQLSERVER_CONNECTION_STRING");
    //connection = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
}

// builder.Services.AddDbContext<PersonDbContext>(options =>
//     options.UseSqlServer(connection));
var AllowedOriginPolicy = "_allowedOriginPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowedOriginPolicy,
        builder =>
        {
            builder.WithOrigins("https://zealous-bay-0ad4fdf10.5.azurestaticapps.net", "http://localhost:4200", "https://notetakerbackend.azurewebsites.net")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddDbContext<Token.Data.TokenContext>(options =>
    options.UseSqlServer(connection));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<UserService>();


var app = builder.Build();
Log.Information("Application started");

app.UseSwagger();
app.UseSwaggerUI();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});


//app.UseHttpsRedirection();

app.UseCors(AllowedOriginPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();

