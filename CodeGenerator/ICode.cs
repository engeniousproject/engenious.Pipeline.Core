namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     Provides a interface for all types of code, which can be written to a <see cref="ICodeBuilder"/>.
    /// </summary>
    public interface ICode
    {
        /// <summary>
        ///     Writes the code to a see <see cref="ICodeBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICodeBuilder"/> to write the code to.</param>
        void WriteTo(ICodeBuilder builder);
    }
}