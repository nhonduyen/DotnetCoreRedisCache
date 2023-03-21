using DotnetCoreRedisCache.Infrastructure.Config;
using DotnetCoreRedisCache.Infrastructure.Data;
using DotnetCoreRedisCache.Services.Implements;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
var redisSetting = configuration.GetSection("Redis").Get<RedisSetting>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDBContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("ProductConnection"), providerOptions => providerOptions.CommandTimeout(120));
});
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = redisSetting.RedisUrl;
//});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { redisSetting.RedisUrl },
    Ssl = redisSetting.Ssl,
    AbortOnConnectFail = redisSetting.AbortOnConnectFail
}));

builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //builder.Services.AddDistributedMemoryCache();
}
else
{
    //builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(new ConfigurationOptions
    //{
    //    EndPoints = { redisSetting.RedisUrl },
    //    Ssl = true,
    //    AbortOnConnectFail = false,
    //}));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
