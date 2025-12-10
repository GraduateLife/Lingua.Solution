using Lingua.Api.Endpoints;
using Lingua.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddLinguaServices();
builder.WebHost.AddLinguaKestrel();

var app = builder.Build();

// Configure middleware
app.UseLinguaMiddleware();

// Map endpoints
app.MapTestEndpoints();
app.MapVideoEndpoints();

app.Run();
