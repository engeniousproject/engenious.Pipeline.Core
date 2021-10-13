using System.ComponentModel;

namespace engenious.ContentTool.Observer
{
    /// <summary>
    /// 
    /// </summary>
    public delegate void NotifyPropertyValueChangedHandler(object sender, PropertyValueChangedEventArgs args);

    /// <summary>
    ///     Notifies clients that a property value has changed and what the previous value as well as the new values are.
    /// </summary>
    public interface INotifyPropertyValueChanged
    {
        /// <summary>Occurs when a property value was changed.</summary>
        event NotifyPropertyValueChangedHandler PropertyValueChanged;
    }
}