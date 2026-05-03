using Common.Authentication;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddOcelot();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI with links to all downstream services
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("http://localhost:5001/swagger/v1/swagger.json", "Identity API");
    c.SwaggerEndpoint("http://localhost:5002/swagger/v1/swagger.json", "Schedule API");
    c.SwaggerEndpoint("http://localhost:5003/swagger/v1/swagger.json", "Appointment API");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();
