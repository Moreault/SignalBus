namespace ToolBX.SignalBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSignalBus(this IServiceCollection services)
    {
        return services.AddScoped<ISignalBus, SignalBus>();
    }
}