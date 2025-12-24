using BLL;
using DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddScoped<Vi_DAL>();
builder.Services.AddScoped<IVi_BLL, Vi_BLL>();
builder.Services.AddScoped<DanhMuc_DAL>();
builder.Services.AddScoped<IDanhMuc_BLL, DanhMuc_BLL>();
builder.Services.AddScoped<GiaoDich_DAL>();
builder.Services.AddScoped<IGiaoDich_BLL, GiaoDich_BLL>();
builder.Services.AddScoped<WalletTransfer_DAL>();
builder.Services.AddScoped<IWalletTransfer_BLL, WalletTransfer_BLL>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("FE", p =>
        p.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
         .AllowAnyHeader()
         .AllowAnyMethod()
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // ✅ relative để mở swagger qua gateway: /walletbudget/swagger/index.html
        c.SwaggerEndpoint("v1/swagger.json", "API_WalletBudget v1");
    });
}

// ✅ CHỈ redirect khi production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FE");

app.MapControllers();
app.Run();
