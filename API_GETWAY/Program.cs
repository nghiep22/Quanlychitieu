using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot();

// CORS (nếu FE gọi gateway)
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("GW", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
});

var app = builder.Build();

app.UseCors("GW");

await app.UseOcelot();

app.Run();
