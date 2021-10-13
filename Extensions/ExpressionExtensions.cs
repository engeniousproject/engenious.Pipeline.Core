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
        /// <returns>The resulting <see cref="TypeReference"/>.</returns>
        public static TypeReference ToTypeReference(this Type type)
        {
            return new TypeReference(type.Namespace, type.Name);
        }
    }
}