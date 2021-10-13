using System;
using System.Collections.ObjectModel;
using engenious.ContentTool.Observer;

namespace engenious.Content.Models
{
    /// <summary>
    ///     An observable collection for content items.
    /// </summary>
    public class ContentItemCollection : ObservableCollection<ContentItem>, INotifyPropertyValueChanged
    {
        /// <inheritdoc />
        public event NotifyPropertyValueChangedHandler? PropertyValueChanged;

        /// <inheritdoc />
        protected override void ClearItems()
        {
            foreach (var i in this)
                i.PropertyValueChanged -= Item_OnPropertyValueChanged;
            base.ClearItems();
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, ContentItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            base.InsertItem(index, item);
            item.PropertyValueChanged += Item_OnPropertyValueChanged;
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            var oldItem = this[index];
            base.RemoveItem(index);

            oldItem.PropertyValueChanged -= Item_OnPropertyValueChanged;
        }

        /// <inheritdoc />
        protected override void SetItem(int index, ContentItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            var oldItem = this[index];
            base.SetItem(index, item);
            oldItem.PropertyValueChanged -= Item_OnPropertyValueChanged;
            item.PropertyValueChanged += Item_OnPropertyValueChanged;
        }

        private void Item_OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            PropertyValueChanged?.Invoke(sender, e);
        }
    }
}