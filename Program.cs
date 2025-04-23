using Microsoft.EntityFrameworkCore;
using SE4458_Midterm_20070006074.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Add API explorer for versioning
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add a swagger document for each discovered API version
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Mobile Provider API V1", 
        Version = "v1",
        Description = "A mobile provider API for managing subscribers and bills."
    });
    
    options.SwaggerDoc("v2", new OpenApiInfo 
    { 
        Title = "Mobile Provider API V2", 
        Version = "v2",
        Description = "Enhanced version with additional features for bill management and reporting."
    });

    // Operation filter to add the api version parameter to each operation
    options.OperationFilter<SwaggerDefaultValues>();
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add DbContext with detailed logging
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        })
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors()
    .LogTo(message => logger.LogInformation(message));
});

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Attempting to ensure database exists and is up to date...");
        
        // Test database connection
        bool canConnect = false;
        try
        {
            canConnect = context.Database.CanConnect();
            logger.LogInformation($"Database connection test result: {canConnect}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error testing database connection: {ex.Message}");
            throw;
        }

        if (canConnect)
        {
            // Recreate database
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            logger.LogInformation("Database recreated successfully");

            // Initialize with seed data
            logger.LogInformation("Initializing database with seed data...");
            DbInitializer.Initialize(context);
            logger.LogInformation("Seed data initialized successfully");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mobile Provider API V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Mobile Provider API V2");
        options.DocumentTitle = "Mobile Provider API Documentation";
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        if (operation.Parameters == null)
            return;

        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
            parameter.Description ??= description.ModelMetadata?.Description;
            
            if (description.RouteInfo != null)
            {
                parameter.Required |= !description.RouteInfo.IsOptional;
            }
        }
    }
}
