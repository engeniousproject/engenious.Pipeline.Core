using System;
using System.Linq;
using System.Linq.Expressions;

namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     Helper class to convert Expression API expressions to <see cref="ICode"/>.
    /// </summary>
    public static class ExpressionConverter
    {
        private static SignatureDefinition GetSignature(LambdaExpression expr)
        {
            return new SignatureDefinition(MethodModifiers.None,
                new TypeReference(expr.ReturnType.Namespace, expr.ReturnType.Name), expr.Name ?? "unnamedTODO",
                expr.Parameters.Select(x => new ParameterDefinition(new TypeReference(x.Type.Namespace, x.Type.Name),
                                           x.Name ?? throw new NotImplementedException()))
                    .ToArray()); // TODO: unnamed functions
        }

        private static string GetOperator(ExpressionType exprType)
        {
            return exprType switch
            {
                ExpressionType.Add => "+",
                ExpressionType.Subtract => "+",
                ExpressionType.Multiply => "*",
                ExpressionType.Divide => "/",
                ExpressionType.AddAssign => "+=",
                ExpressionType.SubtractAssign => "-=",
                ExpressionType.MultiplyAssign => "*=",
                ExpressionType.DivideAssign => "/=",
                _ => exprType + "/* TODO */"
            };
        }

        /// <summary>
        ///     Converts a expression to a generic <see cref="ICode"/>.
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <returns>The converted expression.</returns>
        /// <exception cref="NotSupportedException">Thrown when a conversion is not possible.</exception>
        /// <exception cref="NotImplementedException">Thrown when a conversion was not implemented.</exception>
        public static T Convert<T>(Expression expression)
            where T : ICode
        {
            ICode? res = expression switch
            {
                BlockExpression blockExpr => new BlockExpressionDefinition(
                    new MultilineExpressionDefinition(
                        blockExpr.Expressions.Select(Convert<CodeExpressionDefinition>).ToArray())),
                BinaryExpression binExpr => new BinaryExpressionDefinition(
                    binExpr.Left, binExpr.Right, GetOperator(binExpr.NodeType)),
                LambdaExpression lambda => new ImplementedMethodDefinition(GetSignature(lambda),
                    new MethodBodyDefinition(lambda.Body)),
                ParameterExpression p => new SimpleExpressionDefinition(p.Name ??
                                                                        throw new NotSupportedException(
                                                                            "Unnamed parameters not supported")),
                _ => null
            };

            return res switch
            {
                null => throw new NotImplementedException($"{expression.GetType()} is not implemented!"),
                T definition => definition,
                _ => throw new NotSupportedException($"{expression.GetType()} is not convertible to {typeof(T)}!")
            };
        }
    }
}