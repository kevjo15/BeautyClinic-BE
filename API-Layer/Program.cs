using Domain_Layer.Models;
using Infrastructure_Layer;
using Infrastructure_Layer.Database;
using Microsoft.AspNetCore.Identity;
using Application_Layer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Lägg till tjänster från Application_Layer
builder.Services.AddApplicationLayer();

// Lägg till tjänster från Infrastructure_Layer
builder.Services.AddInfrastructureLayer(builder.Configuration);


// Konfigurera Identity
builder.Services.AddIdentity<UserModel, IdentityRole>()
    .AddEntityFrameworkStores<ElsaBeautyDbContext>()
    .AddDefaultTokenProviders();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
