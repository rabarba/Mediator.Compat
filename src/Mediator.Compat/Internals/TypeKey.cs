namespace MediatR.Internals;

using System;

internal readonly record struct TypeKey(Type Request, Type Response)
{
    public override int GetHashCode() => HashCode.Combine(Request, Response);
}
