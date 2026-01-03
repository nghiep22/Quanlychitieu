using BLL;
using DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<NganSach_DAL>();
builder.Services.AddScoped<INganSach_BLL, NganSach_BLL>();

builder.Services.AddScoped<MucTieuTietKiem_DAL>();
builder.Services.AddScoped<IMucTieuTietKiem_BLL, MucTieuTietKiem_BLL>();

builder.Services.AddScoped<DongGopMucTieu_DAL>();
builder.Services.AddScoped<IDongGopMucTieu_BLL, DongGopMucTieu_BLL>();

builder.Services.AddScoped<ThongBao_DAL>();
builder.Services.AddScoped<IThongBao_BLL, ThongBao_BLL>();


builder.Services.AddScoped<Reports_DAL>();
builder.Services.AddScoped<IReports_BLL, Reports_BLL>();

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
        // ✅ relative để mở swagger qua gateway: /THONGKEBAOCAO/swagger/index.html
        c.SwaggerEndpoint("v1/swagger.json", "API_THONGKEBAOCAO v1");
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

