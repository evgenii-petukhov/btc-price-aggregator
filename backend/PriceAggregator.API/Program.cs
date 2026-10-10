using PriceAggregator.API;
using PriceAggregator.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), optional: false);

builder.Services.AddWebApi(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PricesDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("all");

app.UseRouting();

app.UseHttpsRedirection();

app.UseDefaultFiles();

app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
