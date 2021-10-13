using System;
using System.Linq.Expressions;
using System.Reflection;

namespace engenious.Content.Models.History
{
    /// <summary>
    ///     A history item for a change occuring on a property.
    /// </summary>
    public class HistoryPropertyChange : IHistoryItem
    {
        private readonly object? _oldValue, _newValue;

        private readonly Action<object?> _setter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HistoryPropertyChange"/> class.
        /// </summary>
        /// <param name="reference">The object the property value was changed of.</param>
        /// <param name="propertyName">The name of the property whose value was changed.</param>
        /// <param name="oldValue">The value of the property before the value was changed.</param>
        /// <param name="newValue">The value of the property after the value was changed.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when a property of the given <paramref name="propertyName"/> was not found on the <paramref name="reference"/>.
        /// </exception>
        public HistoryPropertyChange(object reference, string propertyName, object? oldValue, object? newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;

            var prop = reference.GetType().GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var mi = prop?.GetSetMethod();
            if (prop == null || mi == null)
                throw new ArgumentException(
                    $"{reference.GetType()} has no property setter with name {propertyName}");

            var p = Expression.Parameter(typeof(object));
            var call = Expression.Call(Expression.Constant(reference), mi, Expression.Convert(p, prop.PropertyType));
            _setter = Expression.Lambda<Action<object?>>(call, p).Compile();
        }

        /// <inheritdoc />
        public void Undo()
        {
            _setter(_oldValue);
        }

        /// <inheritdoc />
        public void Redo()
        {
            _setter(_newValue);
        }
    }
}