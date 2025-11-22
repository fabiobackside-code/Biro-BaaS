using Adapters.Inbound.REST.DependencyInjection;
using Adapters.Outbound.Persistence.SqlServer;
using bks.sdk.Core.Extensions;
using bks.sdk.Middlewares.Extensions;
using bks.sdk.Observability.Extensions;
using bks.sdk.Security.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add BKS SDK services
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddObservabilityServices(builder.Configuration);
builder.Services.AddSecurityServices(builder.Configuration);

// Add application modules
builder.Services.AddApplicationModule();
builder.Services.AddPersistenceAdapter(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Add BKS SDK middleware
app.UseBKSFrameworkMiddleware();

app.Run();
