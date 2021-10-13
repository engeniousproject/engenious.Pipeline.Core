using System;

namespace engenious.Content.Models.History
{
    /// <summary>
    ///     Provides a interface for a collection of state changes that can be redone and undone.
    /// </summary>
    public interface IHistory : IHistoryItem
    {
        /// <summary>
        ///     Gets a value indicating whether a state change can be undone.
        /// </summary>
        bool CanUndo { get; }
        /// <summary>
        ///     Gets a value indicating whether a state change can be redone.
        /// </summary>
        bool CanRedo { get; }
        /// <summary>
        ///     Occurs when the history state was changed.
        /// </summary>
        event EventHandler? HistoryChanged;
        /// <summary>
        ///     Occurs when a new item was added to the history collection.
        /// </summary>
        event EventHandler? HistoryItemAdded;

        /// <summary>
        ///     Appends a new history item to the history state.
        /// </summary>
        /// <param name="item">The new history item to add.</param>
        void Push(IHistoryItem item);
    }
}