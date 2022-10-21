using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using engenious.ContentTool.Observer;

namespace engenious.Content.Models
{
    /// <summary>
    ///     Class for content items for the content pipeline.
    /// </summary>
    public abstract class ContentItem : INotifyPropertyValueChanged, INotifyPropertyChanged, INotifyCollectionChanged,
        IEquatable<ContentItem>
    {
        private string _name;

        private ContentItem? _parent;

        /// <summary>
        ///     A value indicating whether changed events should be suppressed.
        /// </summary>
        protected bool SuppressChangedEvent = false;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentItem"/> class.
        /// </summary>
        /// <param name="name">Name of the Item</param>
        /// <param name="parent">Parent item</param>
        protected ContentItem(string name, ContentItem? parent)
        {
            _name = name;
            _parent = parent;
        }

        /// <summary>
        ///     Gets the name of the content item.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                var old = _name;
                _name = value;
                OnPropertyChanged(old, value);
            }
        }

        /// <summary>
        ///     Gets or sets a value specifying the errors that occured for this <see cref="ContentItem"/>.
        /// </summary>
        [Browsable(false)] public ContentErrorType Error { get; protected set; }

        /// <summary>
        ///     Gets the path of the content item.
        /// </summary>
        [Browsable(false)]
        public abstract string FilePath { get; }

        /// <summary>
        ///     Gets the relative Path of the content item.
        /// </summary>
        [Browsable(false)]
        public virtual string RelativePath => Parent == null ? Name : Path.Combine(Parent.RelativePath, Name);

        /// <summary>
        ///     Gets parent item this <see cref="ContentItem"/> is a child of; or <c>null</c> if this is the root element.
        /// </summary>
        [Browsable(false)]
        public virtual ContentItem? Parent
        {
            get => _parent;
            set
            {
                if (value == _parent) return;
                var old = _parent;
                _parent = value;
                OnPropertyChanged(old, value);
            }
        }

        /// <summary>
        ///     Gets the content project this <see cref="ContentItem"/> is contained in.
        /// </summary>
        [Browsable(false)]
        public virtual ContentProject Project
        {
            get
            {
                var x = this;
                ContentProject? proj = null;
                while ((proj = x as ContentProject) == null)
                {
                    x = x?.Parent;
                    if (x == null)
                        break;
                }

                return proj ??
                       throw new InvalidOperationException(
                           "Every item, which is not a ContentProject, should have a root Project.");
            }
        }

        /// <inheritdoc />
        public bool Equals(ContentItem? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FilePath == other.FilePath;
        }

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => _notifyPropertyChanged += value;
            remove => _notifyPropertyChanged -= value;
        }

        /// <inheritdoc />
        public event NotifyPropertyValueChangedHandler? PropertyValueChanged;

        /// <summary>
        ///     Serializes the item
        /// </summary>
        public abstract ContentItem Deserialize(XElement element);

        /// <summary>
        ///     Deserializes the item
        /// </summary>
        public abstract XElement Serialize();

        /// <summary>
        ///     Raises <see cref="INotifyPropertyChanged.PropertyChanged"/>
        ///     and <see cref="INotifyPropertyValueChanged.PropertyValueChanged"/> events.
        /// </summary>
        /// <param name="sender">The object that raised the events.</param>
        /// <param name="args">The arguments for the events.</param>
        protected virtual void OnPropertyChanged(object sender, PropertyValueChangedEventArgs args)
        {
            if (SuppressChangedEvent) return;
            PropertyValueChanged?.Invoke(sender, args);
            _notifyPropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(args.PropertyName));
        }

        /// <summary>
        ///     Raises <see cref="INotifyPropertyChanged.PropertyChanged"/>
        ///     and <see cref="INotifyPropertyValueChanged.PropertyValueChanged"/> events.
        /// </summary>
        /// <param name="sender">The object that raised the events.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <param name="propertyName">The name of the property that was changed.</param>
        protected virtual void OnPropertyChanged(object sender, object? oldValue, object? newValue,
            [CallerMemberName] string? propertyName = null)
        {
            if (SuppressChangedEvent) return;
            OnPropertyChanged(sender, new PropertyValueChangedEventArgs(propertyName!, oldValue, newValue));
        }

        /// <summary>
        ///     Raises <see cref="INotifyPropertyChanged.PropertyChanged"/>
        ///     and <see cref="INotifyPropertyValueChanged.PropertyValueChanged"/> events.
        /// </summary>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <param name="propertyName">The name of the property that was changed.</param>
        protected virtual void OnPropertyChanged(object? oldValue, object? newValue,
            [CallerMemberName] string? propertyName = null)
        {
            if (SuppressChangedEvent) return;
            OnPropertyChanged(this, oldValue, newValue, propertyName);
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the events.</param>
        /// <param name="args">The arguments for the event.</param>
        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (SuppressChangedEvent) return;
            CollectionChanged?.Invoke(sender is ContentItem ? sender : this, args);
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the events.</param>
        /// <param name="action">The change action type that occured for the <paramref name="element"/>.</param>
        /// <param name="element">The element that was changed.</param>
        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedAction action, object element)
        {
            OnCollectionChanged(sender, new NotifyCollectionChangedEventArgs(action, element));
        }

        private event PropertyChangedEventHandler? _notifyPropertyChanged;

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ContentItem)obj);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FilePath;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return FilePath.GetHashCode();
        }
    }

    /// <summary>
    ///     Content specific errors of the content pipeline.
    /// </summary>
    [Flags]
    public enum ContentErrorType
    {
        /// <summary>
        ///     No error occured.
        /// </summary>
        None = 1,
        /// <summary>
        ///     Content was not found while trying to load it.
        /// </summary>
        NotFound = 2,
        /// <summary>
        ///     An error occured on trying to import content.
        /// </summary>
        ImporterError = 4,
        /// <summary>
        ///     An error occured on trying to process content.
        /// </summary>
        ProcessorError = 8,
        /// <summary>
        ///     Some other unknown error occured.
        /// </summary>
        Other = 16
    }
}