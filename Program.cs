using Microsoft.EntityFrameworkCore;
using Server.Data;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы в контейнер
builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Настройка конвейера HTTP запросов
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Инициализация тестовых данных
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Tasks.AddRange(
        new Server.Models.Task { Id = Guid.NewGuid(), Title = "Разработка API", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5), Progress = 30 },
        new Server.Models.Task { Id = Guid.NewGuid(), Title = "Создание интерфейса", StartDate = DateTime.Today.AddDays(2), EndDate = DateTime.Today.AddDays(8), Progress = 10 }
    );
    db.SaveChanges();
}

app.Run();