# Mediator.Compat

Lightweight, **drop-in replacement** for MediatR’s core API (no external scanning dependency).
Keep your existing `using MediatR;` code and handler naming — just swap the package.

## Status
Early work-in-progress. Public API contracts first, then core mediator, then DI & scanning.

## Goals
- Drop-in contracts (`IRequest<>`, `IMediator`, `INotification`, `IPipelineBehavior<>`, `Unit`)
- Simple DI registration, reflection-based scanning (Scrutor-free)
- Predictable pipeline order (registration order = outer → inner)
- Clear errors; solid tests; later: delegate/pipeline caching

## License
MIT © Ugur Kap
