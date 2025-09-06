using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Compat.Tests;

// ---- Fixtures ----
public sealed record VoidCmd() : IRequest<Unit>;
public sealed record Note(string Message) : INotification;

public sealed class NoteHandler1 : INotificationHandler<Note>
{
    public static List<string> Calls { get; } = new();
    public Task Handle(Note notification, CancellationToken ct)
    {
        Calls.Add("H1:" + notification.Message);
        return Task.CompletedTask;
    }
}
public sealed class NoteHandler2 : INotificationHandler<Note>
{
    public static List<string> Calls { get; } = new();
    public Task Handle(Note notification, CancellationToken ct)
    {
        Calls.Add("H2:" + notification.Message);
        return Task.CompletedTask;
    }
}

// ---- Tests ----
public class ErrorAndPublishTests
{
    [Fact]
    public async Task Missing_handler_throws_meaningful_exception()
    {
        var sc = new ServiceCollection();

        sc.AddMediatorCompat(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(object).Assembly); // "blank" scan
        });

        var sp = sc.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(new VoidCmd()));

        Assert.Contains("IRequestHandler", ex.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(VoidCmd), ex.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(Unit), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Publish_with_no_handlers_is_noop()
    {
        var sc = new ServiceCollection();

        sc.AddMediatorCompat(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(object).Assembly); // no handlers
        });

        var sp = sc.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Should not throw
        await mediator.Publish(new Note("hello"));
    }

    private static readonly string[] Expected = ["H1:n"];
    private static readonly string[] ExpectedArray = ["H2:n"];

    [Fact]
    public async Task Publish_invokes_all_handlers_sequentially()
    {
        NoteHandler1.Calls.Clear();
        NoteHandler2.Calls.Clear();

        var sc = new ServiceCollection();

        sc.AddMediatorCompat(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(object).Assembly);
        });

        sc.AddTransient<INotificationHandler<Note>, NoteHandler1>();
        sc.AddTransient<INotificationHandler<Note>, NoteHandler2>();

        var sp = sc.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        await mediator.Publish(new Note("n"));

        Assert.Equal(Expected,      NoteHandler1.Calls);
        Assert.Equal(ExpectedArray, NoteHandler2.Calls);
    }
}
