using System;
using System.Collections.Generic;

namespace engenious.Content.Models.History
{
    /// <summary>
    ///     A tree structure to collect history changes; to be able to undo and redo them.
    /// </summary>
    public class HistoryUnion : IHistory
    {
        private readonly List<IHistory> _histories;

        private bool _isWorking;
        private Stack<IHistory> _undo, _redo;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HistoryUnion"/> class.
        /// </summary>
        public HistoryUnion()
        {
            _histories = new List<IHistory>();
            _undo = new Stack<IHistory>();
            _redo = new Stack<IHistory>();
        }

        /// <inheritdoc />
        public event EventHandler? HistoryChanged;

        /// <inheritdoc />
#pragma warning disable CS0067
        public event EventHandler? HistoryItemAdded;
#pragma warning restore CS0067

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


        /// <inheritdoc />
        public void Push(IHistoryItem item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool CanUndo => _undo.Count > 0 && _undo.Peek().CanUndo;

        /// <inheritdoc />
        public bool CanRedo => _redo.Count > 0 && _redo.Peek().CanRedo;

        /// <summary>
        ///     Adds  a history change to this tree collection of history changes.
        /// </summary>
        /// <param name="history">The history change to append.</param>
        public void Add(IHistory history)
        {
            _histories.Add(history);
            history.HistoryItemAdded += HistoryOnHistoryItemAdded;
        }

        /// <summary>
        ///     Removes history change to this tree collection of history changes.
        /// </summary>
        /// <param name="history">The history change to remove.</param>
        public void Remove(IHistory history)
        {
            history.HistoryItemAdded -= HistoryOnHistoryItemAdded;
            _histories.Remove(history);


            RemoveFromStack(ref _undo, history);
            RemoveFromStack(ref _redo, history);
        }

        private static void RemoveFromStack<T>(ref Stack<T> stack, T remove) where T : class
        {
            var tmp = new Stack<T>();
            while (stack.Count > 0)
            {
                var item = stack.Pop();
                if (item != remove)
                    tmp.Push(item);
            }

            stack = tmp;
        }

        private void HistoryOnHistoryItemAdded(object sender, EventArgs eventArgs)
        {
            if (_isWorking)
                return;
            var history = (IHistory)sender;
            _undo.Push(history);
        }
    }
}