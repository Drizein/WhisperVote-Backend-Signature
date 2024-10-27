using Application.Interfaces;
using Application.UseCases;
using Application.Clients;
using Application.Utils;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public static class ConfigureService
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<UCProcessMessage, UCProcessMessage>();
        services.AddTransient<UCGenerateKey, UCGenerateKey>();
        services.AddScoped<IRSAUtil, RSAUtil>();
        services.AddScoped<IAuthServerClient, AuthServerClient>();
        services.AddScoped<IVoteServerClient, VoteServerClient>();
        services.AddScoped<IKeyPairRepository, KeyPairRepository>();
        services.AddScoped<IUserVoteRepository, UserVoteRepository>();
        services.AddScoped<CDbContext, CDbContext>();
        return services;
    }
}