using Magnise_Test_Task.Data;
using Magnise_Test_Task.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddSwagger();
builder.Services.AddHttpClient();
builder.Services.AddCustomServices();
builder.Services.AddHostedServices();
builder.Services.AddDbContexts(builder.Configuration);
builder.Services.AddControllersWithCustomOptions();

var app = builder.Build();

ConfigureDatabaseInitialization(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void ConfigureDatabaseInitialization(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<MarketDbContext>();

        if (!dbContext.Database.CanConnect())
        {
            dbContext.Database.EnsureCreated();
        }
    }
}