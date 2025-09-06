#nullable enable

using System;

namespace MediatR
{
    /// <summary>
    /// Represents a void-like value for requests that do not return a meaningful result.
    /// </summary>
    /// <remarks>
    /// All <see cref="Unit"/> values are equal; use <see cref="Value"/> as the canonical instance.
    /// </remarks>
    public readonly struct Unit : IEquatable<Unit>
    {
        /// <summary>
        /// The canonical instance of <see cref="Unit"/>.
        /// </summary>
        public static readonly Unit Value = new Unit();

        /// <inheritdoc />
        public override string ToString() => "Unit";

        /// <inheritdoc />
        public bool Equals(Unit other) => true; // every Unit equals every other Unit

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Unit;

        /// <inheritdoc />
        public override int GetHashCode() => 0;

        public static bool operator ==(Unit left, Unit right) => true;

        public static bool operator !=(Unit left, Unit right) => false;
    }
}
