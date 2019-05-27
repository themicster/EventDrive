using Microsoft.Extensions.DependencyInjection;

namespace EventDriveCore.Services
{
    public static class Extensions
    {
        public static IMessageBroker UseDefaultMessageBroker(this IServiceCollection services)
        {
            DefaultMessageBroker mb = DefaultMessageBroker.Instance;
            services.AddSingleton<IMessageBroker>(mb);
            return mb;
        }
        
    }
}