#nullable enable

using System.Reflection;
using Mediator.Compat.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mediator.Compat
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
        private static void RegisterCore(IServiceCollection services)
        {
            services.TryAddSingleton<RequestExecutorCache>();
            services.TryAddScoped<IMediator, Mediator>();
        }

        /// <summary>
        /// Minimal overload: register IMediator and scan the given assemblies (handlers  &amp; notifications).
        /// Behaviors are not auto-registered; add them explicitly via <see cref="AddOpenBehavior"/>.
        /// </summary>
        public static IServiceCollection AddMediatorCompat(this IServiceCollection services, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);

            RegisterCore(services);

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

            RegisterCore(services);

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
            var types = assembly.DefinedTypes;

            RegisterClosedHandlers(services, types);
            RegisterOpenGenericHandlers(services, types);
        }

        private static void RegisterClosedHandlers(IServiceCollection services, IEnumerable<TypeInfo> types)
        {
            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition) continue;

                foreach (var iface in type.ImplementedInterfaces)
                {
                    if (!iface.IsGenericType) continue;

                    var def = iface.GetGenericTypeDefinition();
                    if (def != typeof(IRequestHandler<,>) && def != typeof(INotificationHandler<>)) continue;

                    TryAddTransientOnce(services, iface, type.AsType());
                }
            }
        }

        private static void RegisterOpenGenericHandlers(IServiceCollection services, IEnumerable<TypeInfo> types)
        {
            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract || !type.IsGenericTypeDefinition) continue;

                foreach (var iface in type.ImplementedInterfaces)
                {
                    if (!iface.IsGenericType) continue;

                    var def = iface.GetGenericTypeDefinition();
                    Type? openService =
                        def == typeof(INotificationHandler<>) ? typeof(INotificationHandler<>) :
                        def == typeof(IRequestHandler<,>)     ? typeof(IRequestHandler<,>)     : null;

                    if (openService is null) continue;

                    TryAddTransientOnce(services, openService, type.AsType());
                }
            }
        }

        private static void TryAddTransientOnce(IServiceCollection services, Type service, Type impl)
        {
            if (!services.Any(d =>
                    d.Lifetime == ServiceLifetime.Transient &&
                    d.ServiceType == service &&
                    d.ImplementationType == impl))
            {
                services.AddTransient(service, impl);
            }
        }
    }
}
