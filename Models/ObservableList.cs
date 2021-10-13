using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace engenious.Content.Models
{
    /// <summary>
    ///     A list which notifies on changes of the list content as well as influenced properties.
    /// </summary>
    /// <typeparam name="T">The generic type of the list content.</typeparam>
    public class ObservableList<T> : INotifyCollectionChanged, IList<T>, INotifyPropertyChanged
    {
        private readonly List<T> _list;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableList{T}"/> class.
        /// </summary>
        public ObservableList()
        {
            _list = new List<T>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableList{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        public ObservableList(int capacity)
        {
            _list = new List<T>(capacity);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableList{T}"/> class.
        /// </summary>
        /// <param name="collection">Enumeration of items to copy into the initial list.</param>
        public ObservableList(IEnumerable<T> collection)
        {
            _list = new List<T>(collection);
        }

        #region IEnumerable implementation

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion


        #region INotifyCollectionChanged implementation

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        #endregion

        #region INotifyPropertyChanged implementation

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion


        private void RemoveChangedEvents(T item)
        {
            RemoveCollectionChanged(item as INotifyCollectionChanged);
            RemovePropertyChanged(item as INotifyPropertyChanged);
        }

        private void AddChangedEvents(T item)
        {
            AddCollectionChanged(item as INotifyCollectionChanged);
            AddPropertyChanged(item as INotifyPropertyChanged);
        }

        private void RemovePropertyChanged(INotifyPropertyChanged? item)
        {
            if (item == null)
                return;

            item.PropertyChanged -= Item_PropertyChanged;
        }

        private void AddPropertyChanged(INotifyPropertyChanged? item)
        {
            if (item == null)
                return;

            item.PropertyChanged += Item_PropertyChanged;
        }

        private void AddCollectionChanged(INotifyCollectionChanged? item)
        {
            if (item == null)
                return;

            item.CollectionChanged += Item_CollectionChanged;
        }

        private void RemoveCollectionChanged(INotifyCollectionChanged? item)
        {
            if (item == null)
                return;

            item.CollectionChanged -= Item_CollectionChanged;
        }

        private void Item_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(sender, e);
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        #region IList implementation

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }


        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            AddChangedEvents(item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            var old = _list[index];
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old, index));
            _list.RemoveAt(index);
            RemoveChangedEvents(old);
        }

        /// <inheritdoc />
        public T this[int index]
        {
            get => _list[index];
            set
            {
                var old = _list[index];
                _list[index] = value;
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, old));
                RemoveChangedEvents(old);
                AddChangedEvents(value);
            }
        }

        #endregion

        #region ICollection implementation

        /// <inheritdoc />
        public void Add(T item)
        {
            _list.Add(item);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _list.Count - 1));
            AddChangedEvents(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var i in _list)
                RemoveChangedEvents(i);
            _list.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index == -1)
                return false;
            RemoveAt(index);
            return true;
        }

        /// <inheritdoc />
        public int Count => _list.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        #endregion
    }
}