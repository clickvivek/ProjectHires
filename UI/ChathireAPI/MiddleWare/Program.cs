using Microsoft.EntityFrameworkCore;
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
builder.Services.AddDbContext<DataAccessLayer.Models.EFContexts>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var config = new AutoMapper.MapperConfiguration(
    cfg =>
    {
        cfg.AddProfile(new AutoMapperProfile());
        //cf.ValidateInlineMaps = false;
    });
builder.Services.AddSingleton(config.CreateMapper());
builder.Services.AddHttpClient<BusinessLayer.Services.IResendEmailService, BusinessLayer.Services.ResendEmailService>();
builder.Services.AddHttpClient<BusinessLayer.Services.ILinkedInScraperService, BusinessLayer.Services.LinkedInScraperService>();
builder.Services.AddHttpClient<BusinessLayer.Services.IJobParserService, BusinessLayer.Services.JobParserService>();
builder.Services.AddHttpClient<BusinessLayer.Services.IHotlistParserService, BusinessLayer.Services.HotlistParserService>();
builder.Services.AddScoped<DataAccessLayer.Repository.IEmailJobPostingRepository, DataAccessLayer.Repository.EmailJobPostingRepository>();
builder.Services.AddScoped<BusinessLayer.Manager.IEmailJobPostingManager, BusinessLayer.Manager.EmailJobPostingManager>();
builder.Services.AddScoped<DataAccessLayer.Repository.IPromocodeRepository, DataAccessLayer.Repository.PromocodeRepository>();
builder.Services.AddScoped<BusinessLayer.Manager.IPromocodeManager, BusinessLayer.Manager.PromocodeManager>();
builder.Services.AddScoped<BusinessLayer.Manager.IUserReferralManager, BusinessLayer.Manager.UserReferralManager>();
DependancyManager.ConfigureAPI(builder.Services);
builder.Services.AddHostedService<MiddleWare.BackgroundServices.JobExpirationBackgroundService>();

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