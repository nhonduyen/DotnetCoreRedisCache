using DotnetCoreRedisCache.Infrastructure.Config;
using DotnetCoreRedisCache.Infrastructure.Data;
using DotnetCoreRedisCache.Services.Implements;
using DotnetCoreRedisCache.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RedLockNet.SERedis.Configuration;
using RedLockNet.SERedis;
using RedLockNet;
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

//option 1: IDistributedCacheService
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisSetting.RedisUrl;
});

// option 2: IRedisCacheService
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { redisSetting.RedisUrl },
    Ssl = redisSetting.Ssl,
    AbortOnConnectFail = redisSetting.AbortOnConnectFail
}));

builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
builder.Services.AddScoped<IDistributedCacheService, DistributedCacheService>();

// Add RedLock
builder.Services.AddSingleton<IDistributedLockFactory>(provider =>
{
    var multiplexers = new List<RedLockMultiplexer>
    {
        ConnectionMultiplexer.Connect(redisSetting.RedisUrl)
    };
    return RedLockFactory.Create(multiplexers);
});

// Add our service
builder.Services.AddSingleton<IRequestCoalescingService, RequestCoalescingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
