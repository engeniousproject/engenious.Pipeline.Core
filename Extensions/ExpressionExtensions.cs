using System;
using engenious.Content.CodeGenerator;

namespace engenious.Pipeline.Helper
{
    public static class ExpressionExtensions
    {
        public static TypeReference ToTypeReference(this Type type)
        {
            return new TypeReference(type.Namespace, type.Name);
        }
    }
}