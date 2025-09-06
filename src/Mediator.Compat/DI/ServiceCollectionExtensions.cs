#nullable enable

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR
{
    /// <summary>
    /// Options for configuring Mediator.Compat registration without external scanning libs.
    /// </summary>
    public sealed class MediatorCompatOptions
    {
        internal readonly List<Assembly> Assemblies = [];
        internal readonly List<Type> OpenBehaviors = [];

        /// <summary>Add an assembly to scan for IRequestHandler&lt;,&gt; and INotificationHandler&lt;&gt;.</summary>
        public MediatorCompatOptions RegisterServicesFromAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            Assemblies.Add(assembly);
            return this;
        }

        /// <summary>Add an open-generic pipeline behavior (e.g., typeof(ValidationBehavior&lt;,&gt;)).</summary>
        public MediatorCompatOptions AddOpenBehavior(Type openBehavior)
        {
            ArgumentNullException.ThrowIfNull(openBehavior);
            if (!openBehavior.IsGenericTypeDefinition)
                throw new ArgumentException("Provide an open generic type (e.g., typeof(Behavior<,>)).", nameof(openBehavior));

            OpenBehaviors.Add(openBehavior);
            return this;
        }
    }

    /// <summary>
    /// Dependency Injection helpers for Mediator.Compat.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Minimal overload: register IMediator and scan the given assemblies (handlers & notifications).
        /// Behaviors are not auto-registered; add them explicitly via <see cref="AddOpenBehavior"/>.
        /// </summary>
        public static IServiceCollection AddMediatorCompat(this IServiceCollection services, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);

            // Core mediator
            services.AddSingleton<IMediator, Mediator>();

            // Fallback to the calling assembly if none provided
            var toScan = assemblies is { Length: > 0 }
                ? assemblies
                : [Assembly.GetCallingAssembly()];

            foreach (var asm in toScan)
                RegisterFromAssembly(services, asm);

            return services;
        }

        /// <summary>
        /// Full control overload (MediatR-like): configure assemblies to scan and open-generic behaviors in order.
        /// Registration order == behavior execution order (first = outermost).
        /// </summary>
        public static IServiceCollection AddMediatorCompat(this IServiceCollection services, Action<MediatorCompatOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.AddSingleton<IMediator, Mediator>();

            var opts = new MediatorCompatOptions();
            configure(opts);

            var toScan = opts.Assemblies.Count > 0 ? opts.Assemblies.ToArray() : [Assembly.GetCallingAssembly()];
            foreach (var asm in toScan)
                RegisterFromAssembly(services, asm);

            foreach (var open in opts.OpenBehaviors)
                services.AddTransient(typeof(IPipelineBehavior<,>), open);

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
