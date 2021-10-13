namespace engenious.Content.Models.History
{
    /// <summary>
    ///     Provides a interface for states that can be undone and redone.
    /// </summary>
    public interface IHistoryItem
    {
        /// <summary>
        ///     Undoes the state change of an this <see cref="IHistoryItem"/>.
        /// </summary>
        void Undo();

        /// <summary>
        ///     Redoes the state change of an this <see cref="IHistoryItem"/>, that was undone with <see cref="Undo"/> before.
        /// </summary>
        void Redo();
    }
}