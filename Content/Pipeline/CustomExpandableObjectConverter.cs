using System;
using System.ComponentModel;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     An expandable <see cref="TypeConverter"/> for arbitrary properties. 
    /// </summary>
    public class CustomExpandableObjectConverter : TypeConverter
    {
        /// <inheritdoc />
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value,
            Attribute[] attributes)
        {
            var props = TypeDescriptor.GetProperties(value, attributes);
            var ret = new PropertyDescriptor[props.Count];
            for (var i = 0; i < props.Count; i++)
                ret[i] = new SafeTypeDescriptorWrapper(props[i]);
            return new PropertyDescriptorCollection(ret);
        }

        /// <inheritdoc />
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    /// <summary>
    ///     A safe wrapper for <see cref="PropertyDescriptor"/>.
    /// </summary>
    public class SafeTypeDescriptorWrapper : PropertyDescriptor
    {
        /// <summary>
        ///     Occurs when an exception was thrown while trying to work on a property.
        /// </summary>
        public static EventHandler? WorkaroundEvent;
        private readonly PropertyDescriptor _baseDescriptor;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="SafeTypeDescriptorWrapper"/> class.
        /// </summary>
        /// <param name="baseDescriptor">The base <see cref="PropertyDescriptor"/> to wrap.</param>
        public SafeTypeDescriptorWrapper(PropertyDescriptor baseDescriptor)
            : base(baseDescriptor)
        {
            _baseDescriptor = baseDescriptor;
        }

        /// <inheritdoc />
        public override Type ComponentType => _baseDescriptor.ComponentType;

        /// <inheritdoc />
        public override bool IsReadOnly => _baseDescriptor.IsReadOnly;

        /// <inheritdoc />
        public override Type PropertyType => _baseDescriptor.PropertyType;


        /// <inheritdoc />
        public override bool CanResetValue(object? component)
        {
            return component != null && _baseDescriptor.CanResetValue(component);
        }

        /// <inheritdoc />
        public override object? GetValue(object? component)
        {
            return component == null ? null : _baseDescriptor.GetValue(component);
        }

        /// <inheritdoc />
        public override void ResetValue(object? component)
        {
            if (component == null)
                return;

            _baseDescriptor.ResetValue(component);
        }

        /// <inheritdoc />
        public override void SetValue(object? component, object value)
        {
            if (component == null)
                return;

            _baseDescriptor.SetValue(component, value);
        }

        /// <inheritdoc />
        public override bool ShouldSerializeValue(object component)
        {
            try
            {
                return _baseDescriptor.ShouldSerializeValue(component);
            }
            catch (ArgumentNullException)
            {
                WorkaroundEvent?.Invoke(this, EventArgs.Empty);
                return false;
            }
        }
    }
}