using QL_NganQuy.Database;
using QL_NganQuy.Repositories;
using QL_NganQuy.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton(new DbConnection(connectionString));

// Register Repositories
builder.Services.AddScoped<IThongBaoRepository, ThongBaoRepository>();
builder.Services.AddScoped<IBaoCaoRepository, BaoCaoRepository>();
builder.Services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
builder.Services.AddScoped<IViRepository, ViRepository>();

// Register Services
builder.Services.AddScoped<IThongBaoService, ThongBaoService>();
builder.Services.AddScoped<IBaoCaoService, BaoCaoService>();
builder.Services.AddScoped<ITaiKhoanService, TaiKhoanService>();
builder.Services.AddScoped<IViService, ViService>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();