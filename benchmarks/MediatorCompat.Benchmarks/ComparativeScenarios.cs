using BenchmarkDotNet.Attributes;
using Mediator.Compat;
using Microsoft.Extensions.DependencyInjection;

public enum Lib { Compat, MediatR }

[MemoryDiagnoser]
public class ComparativeScenarios
{
    [Params(Lib.Compat, Lib.MediatR)]
    public Lib Library { get; set; }

    [Params(0, 2)]
    public int BehaviorCount { get; set; }

    [Params(0, 2)]
    public int NotificationHandlerCount { get; set; }

    private Func<Task<int>> _sendPing = default!;
    private Func<Task> _sendVoid = default!;
    private Func<Task> _publishNote = default!;

    [GlobalSetup]
    public void Setup()
    {
        var sc = new ServiceCollection();

        if (Library == Lib.Compat)
        {
            sc.AddMediatorCompat(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(object).Assembly);
                for (int i = 0; i < BehaviorCount; i++)
                    cfg.AddOpenBehavior(typeof(Bench.Messages.Compat.BenchmarkBehavior<,>));
            });

            sc.AddTransient<Mediator.Compat.IRequestHandler<Bench.Messages.Compat.Ping, int>, Bench.Messages.Compat.PingHandler>();
            sc.AddTransient<Mediator.Compat.IRequestHandler<Bench.Messages.Compat.VoidCmd, Unit>, Bench.Messages.Compat.VoidHandler>();

            if (NotificationHandlerCount == 2)
            {
                sc.AddTransient<Mediator.Compat.INotificationHandler<Bench.Messages.Compat.Note>, Bench.Messages.Compat.NoteHandler1>();
                sc.AddTransient<Mediator.Compat.INotificationHandler<Bench.Messages.Compat.Note>, Bench.Messages.Compat.NoteHandler2>();
            }

            var sp = sc.BuildServiceProvider();
            var mediator = sp.GetRequiredService<Mediator.Compat.IMediator>();

            _sendPing   = () => mediator.Send(new Bench.Messages.Compat.Ping(41));
            _sendVoid   = () => mediator.Send(new Bench.Messages.Compat.VoidCmd());
            _publishNote= () => mediator.Publish(new Bench.Messages.Compat.Note("n"));
        }
        else
        {
            sc.AddLogging();
            sc.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(object).Assembly);
            });

            for (int i = 0; i < BehaviorCount; i++)
                sc.AddTransient(typeof(MediatR.IPipelineBehavior<,>),
                    typeof(Bench.Messages.Official.BenchmarkBehavior<,>));

            sc.AddTransient<
                MediatR.IRequestHandler<Bench.Messages.Official.Ping, int>,
                Bench.Messages.Official.PingHandler>();

            sc.AddTransient<
                MediatR.IRequestHandler<Bench.Messages.Official.VoidCmd, MediatR.Unit>,
                Bench.Messages.Official.VoidHandler>();

            if (NotificationHandlerCount == 2)
            {
                sc.AddTransient<
                    MediatR.INotificationHandler<Bench.Messages.Official.Note>,
                    Bench.Messages.Official.NoteHandler1>();
                sc.AddTransient<
                    MediatR.INotificationHandler<Bench.Messages.Official.Note>,
                    Bench.Messages.Official.NoteHandler2>();
            }

            var sp = sc.BuildServiceProvider();
            var mediator = sp.GetRequiredService<MediatR.IMediator>();

            _sendPing    = () => mediator.Send(new Bench.Messages.Official.Ping(41));
            _sendVoid    = () => mediator.Send(new Bench.Messages.Official.VoidCmd());
            _publishNote = () => mediator.Publish(new Bench.Messages.Official.Note("n"));
        }
    }

    [Benchmark] public Task<int> Send_Ping()   => _sendPing();
    [Benchmark] public Task      Send_Void()   => _sendVoid();
    [Benchmark] public Task      Publish_Note()=> _publishNote();
}
