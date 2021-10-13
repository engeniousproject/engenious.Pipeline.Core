using System.ComponentModel;

namespace engenious.ContentTool.Observer
{
    /// <summary>
    ///     Provides data for the <see cref="INotifyPropertyValueChanged.PropertyValueChanged" /> event.
    /// </summary>
    public class PropertyValueChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueChangedEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        /// <param name="oldValue">The value of the property before it was changed.</param>
        /// <param name="newValue">The new value the property was set to.</param>
        public PropertyValueChangedEventArgs(string propertyName, object? oldValue, object? newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the value the property was changed to.
        /// </summary>
        public object? NewValue { get; }

        /// <summary>
        /// Gets the value of the property before it was changed.
        /// </summary>
        public object? OldValue { get; }
    }
}