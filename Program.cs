using IEEEBackend.Data;
using IEEEBackend.Interfaces;
using IEEEBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using IEEEBackend.Interfaces;   // Eklendi
using IEEEBackend.Repositories; // Eklendi

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Eklendi 
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Eklendi
builder.Services.AddScoped<ICommitteeRepository, CommitteeRepository>();
// Add Repositories
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventPhotoRepository, EventPhotoRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable static file serving
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();