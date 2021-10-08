﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using engenious.ContentTool.Observer;

namespace engenious.Content.Models
{
    public abstract class ContentItem : INotifyPropertyValueChanged, INotifyPropertyChanged,INotifyCollectionChanged, IEquatable<ContentItem>
    {
        /// <summary>
        /// The name of the content item
        /// </summary>
        public string Name { get => _name;
            set
            {
                if (_name == value) return;
                var old = _name;
                _name = value;
                OnPropertyChanged(old,value);
            }
        }

        private string _name;
        
        protected bool SupressChangedEvent = false;

        [Browsable(false)]
        public ContentErrorType Error { get; set; }

        /// <summary>
        /// The path of the content item
        /// </summary>
        [Browsable(false)]
        public abstract string FilePath { get; }

        /// <summary>
        /// The relative Path of the content item
        /// </summary>
        [Browsable(false)]
        public virtual string RelativePath => Path.Combine(Parent.RelativePath, Name);

        /// <summary>
        /// The parent item
        /// </summary>
        [Browsable(false)]
        public virtual ContentItem Parent { get => _parent;
            set
            {
                if (value == _parent) return;
                var old = _parent;
                _parent = value;
                OnPropertyChanged(old,value);
            }
        }
        /// <summary>
        /// The content project
        /// </summary>
        [Browsable(false)]
        public virtual ContentProject Project {
            get
            {
                var x = this;
                ContentProject proj = null;
                while ((proj = (x as ContentProject)) == null)
                {
                    x = x?.Parent;
                    if (x == null)
                        break;
                }
                return proj;
            }
        }

        private ContentItem _parent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the Item</param>
        /// <param name="parent">Parent item</param>
        protected ContentItem(string name, ContentItem parent)
        {
            _name = name;
            _parent = parent;
        }

        /// <summary>
        /// Serializes the item
        /// </summary>
        public abstract ContentItem Deserialize(XElement element);

        /// <summary>
        /// Deserializes the item
        /// </summary>
        public abstract XElement Serialize();


        protected virtual void OnPropertyChanged(object sender,PropertyValueChangedEventArgs args)
        {
            if (SupressChangedEvent) return;
            PropertyValueChanged?.Invoke(sender, args);
            _notifyPropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(args.PropertyName));
        }
        protected virtual void OnPropertyChanged(object sender,object oldValue,object newValue,[CallerMemberName] string propertyName = null)
        {
            if (SupressChangedEvent) return;
            OnPropertyChanged(sender, new PropertyValueChangedEventArgs(propertyName,oldValue,newValue));
        }
        protected virtual void OnPropertyChanged(object oldValue,object newValue,[CallerMemberName] string propertyName = null)
        {
            if (SupressChangedEvent) return;
            OnPropertyChanged(this,oldValue,newValue,propertyName);
        }
        protected virtual void OnCollectionChanged(object sender,NotifyCollectionChangedEventArgs args)
        {
            if (SupressChangedEvent) return;
            if (sender is ContentItem)
                CollectionChanged?.Invoke(sender,args);
            else
                CollectionChanged?.Invoke(this, args);
        }
        protected virtual void OnCollectionChanged(object sender,NotifyCollectionChangedAction action,object element)
        {
             OnCollectionChanged(sender,new NotifyCollectionChangedEventArgs(action,element));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event NotifyPropertyValueChangedHandler PropertyValueChanged;

        private event PropertyChangedEventHandler? _notifyPropertyChanged;
        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => _notifyPropertyChanged += value;
            remove => _notifyPropertyChanged -= value;
        }

        public bool Equals(ContentItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FilePath == other.FilePath;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContentItem) obj);
        }

        public override string ToString()
        {
            return FilePath ?? string.Empty;
        }

        public override int GetHashCode()
        {
            return (FilePath != null ? FilePath.GetHashCode() : 0);
        }
    }

    [Flags]
    public enum ContentErrorType
    {
        None = 1,
        NotFound = 2,
        ImporterError = 4,
        ProcessorError = 8,
        Other = 16
    }
}
