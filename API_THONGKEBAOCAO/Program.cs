using BLL;
using DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddScoped<NganSach_DAL>();
builder.Services.AddScoped<INganSach_BLL, NganSach_BLL>();
builder.Services.AddScoped<MucTieuTietKiem_DAL>();
builder.Services.AddScoped<IMucTieuTietKiem_BLL, MucTieuTietKiem_BLL>();
builder.Services.AddScoped<DongGopMucTieu_DAL>();
builder.Services.AddScoped<IDongGopMucTieu_BLL, DongGopMucTieu_BLL>();
builder.Services.AddScoped<ThongBao_DAL>();
builder.Services.AddScoped<IThongBao_BLL, ThongBao_BLL>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
