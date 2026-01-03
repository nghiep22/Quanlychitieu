using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);

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

// ✅ Host Swagger UI tại /swagger (không generate swagger.json ở gateway)
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";

    // trỏ swagger.json QUA GATEWAY để tránh CORS
    c.SwaggerEndpoint("/walletbudget/swagger/v1/swagger.json", "API_WalletBudget v1");
    c.SwaggerEndpoint("/login/swagger/v1/swagger.json", "API_Login v1");
    c.SwaggerEndpoint("/thongkebaocao/swagger/v1/swagger.json", "API_THONGKEBAOCAO v1");
});

// (tuỳ chọn) vào root thì đá sang /swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

await app.UseOcelot();
app.Run();
