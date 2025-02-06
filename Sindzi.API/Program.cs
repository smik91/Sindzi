using Microsoft.AspNetCore.Builder;
using Sindzi.API.Extensions;
using Microsoft.Extensions.Hosting;
using Sindzi.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();
