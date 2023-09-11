// Copyright (c) Microsoft. All rights reserved.

/*
* Note: This sample app supports only one customer conversation at any given time.
* MemCache is used to keep the active state (customer identity, access-token, threadId, voice callId etc).
* Active state is reset via /debug API or expires after 1h
*/

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddBackendServices();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "ACS Mechanics");
});

app.UseCors(option =>
{
    option.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
