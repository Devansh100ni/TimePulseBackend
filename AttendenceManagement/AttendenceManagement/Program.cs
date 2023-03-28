using AttendenceManagement.Infrastructure.IRepository;
using AttendenceManagement.Infrastructure.Repository;
using AttendenceManagement.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContextPool<AttendanceManagement001Context>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnection"));
});
builder.Services.AddScoped<IDatasheet, Datasheet>();
builder.Services.AddScoped<ILogProcess, LogProcessRepo>();


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

app.Run();
