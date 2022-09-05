using System;
using engenious.Content.CodeGenerator;

namespace engenious.Pipeline.Helper
{
    /// <summary>
    ///     A static extension class for <see cref="Type"/>.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        ///     Converts a <see cref="Type"/> to a <see cref="TypeReference"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to convert.</param>
        /// <param name="nullable">Whether to make the type nullable.</param>
        /// <returns>The resulting <see cref="TypeReference"/>.</returns>
        public static TypeReference ToTypeReference(this Type type, bool nullable = false)
        {
            return new TypeReference(type.Namespace, (!type.Name.EndsWith("?") && nullable) ? type.Name + "?" : type.Name);
        }
        /// <summary>
        ///     Converts a <see cref="Type"/> to a <see cref="TypeReference"/>.
        /// </summary>
        /// <param name="tuple">The tuple to convert.</param>
        /// <returns>The resulting <see cref="TypeReference"/>.</returns>
        public static TypeReference ToTypeReference(this (Type type, bool nullable) tuple)
        {
            return tuple.type.ToTypeReference(tuple.nullable);
        }
    }
}