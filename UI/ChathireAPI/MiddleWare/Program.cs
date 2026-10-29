using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Middleware.Shared;
using Middleware.Security;
using Microsoft.AspNetCore.Authentication;
using Azure.Storage.Blobs;
using BusinessLayer.Manager;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[]
{
    "https://www.chathire.com",
    "https://chathire.com",
    "https://app.chathire.com",
    "http://localhost:4200",
    "https://localhost:4200"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        corsBuilder => corsBuilder
            .WithOrigins(allowedOrigins)
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
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
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<BusinessLayer.Services.IResendEmailService, BusinessLayer.Services.ResendEmailService>();
builder.Services.AddHttpClient<BusinessLayer.Services.ILinkedInScraperService, BusinessLayer.Services.LinkedInScraperService>();
builder.Services.AddHttpClient<BusinessLayer.Services.ICompanyUrlScraperService, BusinessLayer.Services.CompanyUrlScraperService>();
builder.Services.AddHttpClient<BusinessLayer.Services.IJobParserService, BusinessLayer.Services.JobParserService>();
builder.Services.AddHttpClient<BusinessLayer.Services.IHotlistParserService, BusinessLayer.Services.HotlistParserService>();
builder.Services.AddScoped<DataAccessLayer.Repository.IEmailJobPostingRepository, DataAccessLayer.Repository.EmailJobPostingRepository>();
builder.Services.AddScoped<BusinessLayer.Manager.IEmailJobPostingManager, BusinessLayer.Manager.EmailJobPostingManager>();
builder.Services.AddScoped<DataAccessLayer.Repository.IPromocodeRepository, DataAccessLayer.Repository.PromocodeRepository>();
builder.Services.AddScoped<BusinessLayer.Manager.IPromocodeManager, BusinessLayer.Manager.PromocodeManager>();
builder.Services.AddScoped<BusinessLayer.Manager.IUserReferralManager, BusinessLayer.Manager.UserReferralManager>();
builder.Services.AddScoped<DataAccessLayer.Repository.IConfigRepository, DataAccessLayer.Repository.ConfigRepository>();
builder.Services.AddScoped<BusinessLayer.Manager.IConfigManager, BusinessLayer.Manager.ConfigManager>();
builder.Services.AddScoped<DataAccessLayer.Repository.ISearchedRepository, DataAccessLayer.Repository.SearchedRepository>();
builder.Services.AddScoped<BusinessLayer.Manager.ISearchedManager, BusinessLayer.Manager.SearchedManager>();
DependancyManager.ConfigureAPI(builder.Services);
builder.Services.AddHostedService<MiddleWare.BackgroundServices.JobExpirationBackgroundService>();
builder.Services.AddHostedService<MiddleWare.BackgroundServices.ChatCleanupBackgroundService>();

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

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DataAccessLayer.Models.EFContexts>();
        db.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Consultancy]') 
    AND name = 'website2'
)
BEGIN
    ALTER TABLE [dbo].[Consultancy] ADD [website2] VARCHAR(255) NULL;
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CandidateProfileMappingStatus')
BEGIN
    IF (SELECT COUNT(*) FROM [dbo].[CandidateProfileMappingStatus]) = 0
    BEGIN
        SET IDENTITY_INSERT [dbo].[CandidateProfileMappingStatus] ON;
        INSERT INTO [dbo].[CandidateProfileMappingStatus] (Id, Name, Description, Updated, UpdatedBy) VALUES 
            (1, 'New', 'New Application', GETUTCDATE(), 1),
            (2, 'On Hold', 'On Hold', GETUTCDATE(), 1),
            (3, 'Shortlisted', 'Shortlisted', GETUTCDATE(), 1),
            (4, 'No Response', 'No Response', GETUTCDATE(), 1),
            (5, 'Interview', 'Interview', GETUTCDATE(), 1),
            (6, 'Rejected', 'Rejected', GETUTCDATE(), 1),
            (7, 'Submitted', 'Submitted', GETUTCDATE(), 1);
        SET IDENTITY_INSERT [dbo].[CandidateProfileMappingStatus] OFF;
    END
END

-- Auto-generate publicProfileUserName for existing ConsultancyUsers where missing
DECLARE @cuId BIGINT, @uFname NVARCHAR(100), @uLname NVARCHAR(100), @baseHandle NVARCHAR(100), @finalHandle NVARCHAR(100), @suffix INT;

DECLARE cu_cursor CURSOR FOR
    SELECT cu.Id, u.Fname, u.Lname
    FROM [dbo].[ConsultancyUser] cu
    INNER JOIN [dbo].[User] u ON cu.UserId = u.Id
    WHERE (cu.PublicProfileUserName IS NULL OR LTRIM(RTRIM(cu.PublicProfileUserName)) = '')
      AND u.Fname IS NOT NULL AND LTRIM(RTRIM(u.Fname)) <> '';

OPEN cu_cursor;
FETCH NEXT FROM cu_cursor INTO @cuId, @uFname, @uLname;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @baseHandle = LOWER(LTRIM(RTRIM(ISNULL(@uFname, ''))) + LTRIM(RTRIM(ISNULL(@uLname, ''))));
    DECLARE @cleanHandle NVARCHAR(100) = '';
    DECLARE @p INT = 1;
    WHILE @p <= LEN(@baseHandle)
    BEGIN
        DECLARE @c NCHAR(1) = SUBSTRING(@baseHandle, @p, 1);
        IF @c LIKE '[a-z0-9]'
            SET @cleanHandle = @cleanHandle + @c;
        SET @p = @p + 1;
    END

    IF LEN(@cleanHandle) > 0
    BEGIN
        SET @finalHandle = @cleanHandle;
        IF EXISTS (SELECT 1 FROM [dbo].[ConsultancyUser] WHERE PublicProfileUserName = @finalHandle AND Id <> @cuId)
        BEGIN
            SET @suffix = CAST(FLOOR(RAND() * 90 + 10) AS INT);
            SET @finalHandle = @cleanHandle + CAST(@suffix AS NVARCHAR(10));
            WHILE EXISTS (SELECT 1 FROM [dbo].[ConsultancyUser] WHERE PublicProfileUserName = @finalHandle AND Id <> @cuId)
            BEGIN
                SET @suffix = CAST(FLOOR(RAND() * 90 + 10) AS INT);
                SET @finalHandle = @cleanHandle + CAST(@suffix AS NVARCHAR(10));
            END
        END

        UPDATE [dbo].[ConsultancyUser]
        SET PublicProfileUserName = @finalHandle
        WHERE Id = @cuId;
    END

    FETCH NEXT FROM cu_cursor INTO @cuId, @uFname, @uLname;
END

CLOSE cu_cursor;
DEALLOCATE cu_cursor;");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error running Consultancy table migration: {ex.Message}");
    }
}

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