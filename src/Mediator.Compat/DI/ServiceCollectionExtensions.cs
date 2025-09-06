#nullable enable

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR
{
    /// <summary>
    /// Dependency Injection helpers for Mediator.Compat.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers IMediator and scans the given assemblies for IRequestHandler and INotificationHandler.
        /// Behaviors are NOT auto-registered; add them explicitly via AddOpenBehavior(typeof(Behavior)).
        /// Behavior execution order follows registration order (first = outermost).
        /// </summary>
        public static IServiceCollection AddMediatorCompat(this IServiceCollection services, params Assembly[]? assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);

            // Core mediator
            services.AddSingleton<IMediator, Mediator>();

            // Fallback to the calling assembly if none provided
            var toScan = assemblies is { Length: > 0 }
                ? assemblies
                : [Assembly.GetCallingAssembly()];

            foreach (var asm in toScan)
            {
                RegisterFromAssembly(services, asm);
            }

            return services;
        }

        /// <summary>
        /// Adds an open-generic pipeline behavior, e.g. typeof(ValidationBehavior&lt;,&gt;).
        /// </summary>
        public static IServiceCollection AddOpenBehavior(this IServiceCollection services, Type openBehavior)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(openBehavior);

            if (!openBehavior.IsGenericTypeDefinition)
                throw new ArgumentException("Provide an open generic type (e.g., typeof(Behavior<,>)).", nameof(openBehavior));

            services.AddTransient(typeof(IPipelineBehavior<,>), openBehavior);
            return services;
        }

        private static void RegisterFromAssembly(IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.DefinedTypes)
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (type.IsGenericTypeDefinition) continue; // we register open generics explicitly via AddOpenBehavior

                foreach (var iface in type.ImplementedInterfaces)
                {
                    if (!iface.IsGenericType) continue;

                    var def = iface.GetGenericTypeDefinition();
                    var isReq  = def == typeof(IRequestHandler<,>);
                    var isNote = def == typeof(INotificationHandler<>);
                    // Intentionally DO NOT auto-register closed IPipelineBehavior<,> implementations.
                    // Behaviors should be added explicitly to control order, e.g. services.AddOpenBehavior(typeof(B1<,>));
                    if (!(isReq || isNote)) continue;

                    // De-duplicate: avoid adding same (ServiceType, ImplementationType) twice
                    var serviceType = iface;
                    var implType = type.AsType();
                    var already = services.Any(d =>
                        d.ServiceType == serviceType &&
                        d.ImplementationType == implType &&
                        d.Lifetime == ServiceLifetime.Transient);

                    if (!already)
                    {
                        services.AddTransient(serviceType, implType);
                    }
                }
            }
        }
    }
}
