using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Base class for processor specific settings.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    public class ProcessorSettings
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return string.Empty;
        }

        #region Serialization

        private static void SetPrimitive(PropertyDescriptor property, object obj, string? val)
        {
            var code = Type.GetTypeCode(property.PropertyType);
            if (code != TypeCode.String && val == null)
                return;
            switch (code)
            {
                case TypeCode.String:
                    property.SetValue(obj, val);
                    break;
                case TypeCode.Char:
                    property.SetValue(obj, string.IsNullOrEmpty(val) ? '\0' : val![0]);
                    break;
                case TypeCode.SByte:
                {
                    if (sbyte.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                        property.SetValue(obj, num);
                }
                    break;
                case TypeCode.Int16:
                {
                    if (short.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                        property.SetValue(obj, num);
                }
                    break;
                case TypeCode.Int32:
                {
                    if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                        property.SetValue(obj, num);
                }
                    break;
                case TypeCode.Int64:
                {
                    if (long.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                        property.SetValue(obj, num);
                }
                    break;
                case TypeCode.Byte:
                {
                    if (byte.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                        property.SetValue(obj, num);
                }
                    break;
                case TypeCode.UInt16:
                {
                    if (ushort.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                        property.SetValue(obj, num);
                }
                    break;
                case TypeCode.UInt32:
                {
                    if (uint.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                        property.SetValue(obj, num);
                }
                    break;
                case TypeCode.UInt64:
                {
                    if (ulong.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                        property.SetValue(obj, num);
                }
                    break;
                case TypeCode.Boolean:
                {
                    if (bool.TryParse(val, out var tmp))
                        property.SetValue(obj, tmp);
                }
                    break;
                case TypeCode.DateTime:
                {
                    if (DateTime.TryParse(val, out var dt))
                        property.SetValue(obj, dt);
                }
                    break;
                case TypeCode.Single:
                {
                    if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var num))
                        property.SetValue(obj, num);
                }
                    break;
                case TypeCode.Double:
                {
                    if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var num))
                        property.SetValue(obj, num);
                }
                    break;
                case TypeCode.Decimal:
                {
                    if (decimal.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var num))
                        property.SetValue(obj, num);
                }
                    break;
            }
        }

        private static void ReadObject(XElement nodes, object obj)
        {
            var props = TypeDescriptor.GetProperties(obj).OfType<PropertyDescriptor>()
                .ToDictionary(x => x.Name, x => x);
            foreach (var setting in nodes.Elements())
                if (props.TryGetValue(setting.Name.LocalName, out var property))
                {
                    var val = setting.Nodes().OfType<XText>().FirstOrDefault()?.Value;
                    if (val == null)
                        continue;
                    if (property.PropertyType.IsPrimitive)
                    {
                        SetPrimitive(property, obj, val);
                    }
                    else if (property.PropertyType.IsEnum)
                    {
                        try
                        {
                            property.SetValue(obj, Enum.Parse(property.PropertyType, val));
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    else
                    {
                        var tmp = Activator.CreateInstance(property.PropertyType);
                        if (tmp != null)
                        {
                            ReadObject(setting, tmp);
                            property.SetValue(obj, tmp);
                        }
                    }
                }
        }

        /// <summary>
        ///     Reads the processor settings from a xml node.
        /// </summary>
        /// <param name="nodes">The xml node to read from.</param>
        public virtual void Read(XElement nodes)
        {
            ReadObject(nodes, this);
        }

        private static string? PrimitiveToString(object? obj)
        {
            if (obj == null)
                return null;
            var code = Type.GetTypeCode(obj.GetType());
            switch (code)
            {
                case TypeCode.String:
                    return (string)obj;
                case TypeCode.Boolean:
                case TypeCode.Char:
                    return obj.ToString();
                case TypeCode.DateTime:
                    return ((DateTime)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Decimal:
                    return ((decimal)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Double:
                    return ((double)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Single:
                    return ((float)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Int16:
                    return ((short)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Int32:
                    return ((int)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Int64:
                    return ((long)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.UInt16:
                    return ((ushort)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.UInt32:
                    return ((uint)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.UInt64:
                    return ((ulong)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Byte:
                    return ((byte)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.SByte:
                    return ((sbyte)obj).ToString(CultureInfo.InvariantCulture);
                case TypeCode.DBNull:
                case TypeCode.Empty:
                case TypeCode.Object:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void WriteObject(XElement writer, object? obj)
        {
            if (obj == null)
                return;
            foreach (var prop in TypeDescriptor.GetProperties(obj).OfType<PropertyDescriptor>())
            {
                if (prop == null)
                    continue;
                var type = prop.PropertyType;
                if (prop.IsReadOnly)
                    continue;
                if (type.IsPrimitive || type.IsEnum)
                {
                    writer.Add(new XElement(prop.Name, PrimitiveToString(prop.GetValue(obj))));
                }
                else
                {
                    var el = new XElement(prop.Name);
                    WriteObject(el, prop.GetValue(obj));
                    writer.Add(el);
                }
            }
        }

        /// <summary>
        ///     Writes the processor settings to xml.
        /// </summary>
        /// <param name="writer">The xml element to write to.</param>
        public virtual void Write(XElement writer)
        {
            WriteObject(writer, this);
        }

        #endregion
    }
}