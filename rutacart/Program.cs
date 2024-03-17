using Microsoft.EntityFrameworkCore;
using rutacart.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Registra el DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting(); // Asegúrate de llamar a UseRouting antes de aplicar las políticas CORS.

app.UseCors("AllowSpecificOrigin"); // Asegúrate de que este nombre coincida con el nombre de la política definida.

app.UseAuthorization();
builder.Logging.AddConsole();


app.MapControllers();

app.Run();
