using System;
using System.Collections.Generic;

namespace engenious.Content.Models.History
{
    /// <summary>
    ///     Implementation for a collection of state changes that can be undone and redone.
    /// </summary>
    public class History : IHistory
    {
        private readonly Stack<IHistoryItem> _undo, _redo;
        private bool _isWorking;

        /// <summary>
        ///     Initializes a new instance of the <see cref="History"/> class.
        /// </summary>
        public History()
        {
            _undo = new Stack<IHistoryItem>();
            _redo = new Stack<IHistoryItem>();
        }

        /// <inheritdoc />
        public event EventHandler? HistoryChanged;

        /// <inheritdoc />
        public event EventHandler? HistoryItemAdded;

        /// <inheritdoc />
        public void Push(IHistoryItem item)
        {
            if (_isWorking)
                return;
            _undo.Push(item);
            HistoryItemAdded?.Invoke(this, EventArgs.Empty);
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public bool CanUndo => _undo.Count > 0;

        /// <inheritdoc />
        public bool CanRedo => _redo.Count > 0;

        /// <inheritdoc />
        public void Undo()
        {
            var item = _undo.Pop();
            _isWorking = true;
            item.Undo();
            _isWorking = false;
            _redo.Push(item);

            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void Redo()
        {
            var item = _redo.Pop();
            _isWorking = true;
            item.Redo();
            _isWorking = false;
            _undo.Push(item);

            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}