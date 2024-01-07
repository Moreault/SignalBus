namespace ToolBX.SignalBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSignalBus(this IServiceCollection services, AutoInjectOptions? options = null)
    {
        return services.AddAutoInjectServices(Assembly.GetExecutingAssembly(), options);
    }
}