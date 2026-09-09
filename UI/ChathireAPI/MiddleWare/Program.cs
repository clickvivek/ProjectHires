
using Microsoft.OpenApi.Models;
using Middleware.Shared;
using Middleware.Security;
using Microsoft.AspNetCore.Authentication;
using Azure.Storage.Blobs;
using BusinessLayer.Manager;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});
string CustomAuthenticationScheme = "CustomScheme";

builder.Services.AddAuthentication(CustomAuthenticationScheme)
.AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>(CustomAuthenticationScheme, null);

//builder.Services.AddAuthentication("Basic")
//            .AddScheme<BasicAuthenticationOptions, CustomAuthenticationHandler>("Basic", null);

builder.Services.AddMvc(option => option.EnableEndpointRouting = false);
builder.Services.AddTransient<DataAccessLayer.Models.EFContexts>();
var config = new AutoMapper.MapperConfiguration(
    cfg =>
    {
        cfg.AddProfile(new AutoMapperProfile());
        //cf.ValidateInlineMaps = false;
    });
builder.Services.AddSingleton(config.CreateMapper());
DependancyManager.ConfigureAPI(builder.Services);

//builder.Services.AddScoped(_ =>
//{
//    return new BlobServiceClient(builder.Configuration.GetConnectionString("AzureStorage"));
//});

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "REST API",
        Description = "Rest Calls",
    });

    //c.IncludeXmlComments(MiddleWare.Startup.GetXmlCommentsPath());
});

var app = builder.Build();

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "REST API V2");
});
app.UseMvc();

app.Run(async (context) =>
{
    await context.Response.WriteAsync("Invalid Request");
});

app.Run();