// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BDA.Common.Exchange.Configs.Attributes;
using PropertyChanged;

namespace BDA.Common.Exchange.Configs.Helper
{
    #region Hilfsklassen

    /// <summary>
    ///     <para>Infos zum Objekt</para>
    ///     Klasse ConfigGeneratorObject. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConfigGeneratorObject : IComparable<ConfigGeneratorObject>, IComparable
    {
        #region Properties

        /// <summary>
        ///     Beschreibung der Config
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        ///     Type
        /// </summary>
        public Type ConfigType { get; set; } = null!;

        /// <summary>
        ///     Properties
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<ConfigGeneratorProperty> Properties { get; set; } = new();
#pragma warning restore CA2227 // Collection properties should be read only

        #endregion

        #region Interface Implementations

        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is ConfigGeneratorObject other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ConfigGeneratorObject)}");
        }

        /// <inheritdoc />
        public int CompareTo(ConfigGeneratorObject? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return string.Compare(Label, other.Label, StringComparison.Ordinal);
        }

        #endregion
    }

    /// <summary>
    ///     <para>Infos zu den Properties des Object</para>
    ///     Klasse ConfigGeneratorProperty. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConfigGeneratorProperty : IComparable<ConfigGeneratorProperty>, IComparable
    {
        #region Properties

        /// <summary>
        ///     Beschreibung des Felds
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        ///     Property
        /// </summary>
        public PropertyInfo Property { get; set; } = null!;

        /// <summary>
        ///     SortIndex
        /// </summary>
        public int SortIndex { get; set; }

        /// <summary>
        ///     Wert aus CLI oder UI (wird versucht automatisch konvertiert)
        ///     Es funktionieren "nur" "normale" Basistypen
        /// </summary>
        public string Value { get; set; } = null!;

        /// <summary>
        ///     Was wurde alles im Code definiert
        /// </summary>
        public ConfigPropertyAttribute PropertyAttribute { get; set; } = null!;

        /// <summary>
        ///     Ist beim konvertieren ein Fehler aufgetreten
        /// </summary>
        [AlsoNotifyFor(nameof(Error))]
        public string ConvertError { get; set; } = string.Empty;

        /// <summary>
        ///     Fehler bei der konvertierung
        /// </summary>
        public bool Error => !string.IsNullOrEmpty(ConvertError);

        #endregion

        #region Interface Implementations

        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is ConfigGeneratorProperty other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ConfigGeneratorProperty)}");
        }

        /// <inheritdoc />
        public int CompareTo(ConfigGeneratorProperty? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            var sortIndexComparison = SortIndex.CompareTo(other.SortIndex);
            if (sortIndexComparison != 0)
            {
                return sortIndexComparison;
            }

            return string.Compare(Label, other.Label, StringComparison.Ordinal);
        }

        #endregion
    }

    #endregion

    /// <summary>
    ///     <para>Configurationen (inkl. OP-Codes) dynamisch für Db generieren</para>
    ///     Klasse ConfigGenerator. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConfigGenerator<T>
    {
        /// <summary>
        ///     Sucht im aktuellem Assembly nach Typen von T
        /// </summary>
        public ConfigGenerator()
        {
            var currentTypes = Assembly.GetExecutingAssembly().GetTypes().ToList();
            foreach (var type in currentTypes)
            {
                if (CheckSearchedType(type))
                {
                    var configAttribute = type.GetCustomAttribute<ConfigNameAttribute>();
                    if (configAttribute != null!)
                    {
                        var data = new ConfigGeneratorObject
                        {
                            Label = configAttribute.Label,
                            ConfigType = type
                        };
                        foreach (var property in type.GetProperties())
                        {
                            var propertyAttribute = property.GetCustomAttribute<ConfigPropertyAttribute>();
                            //ToDo Mko IList prüfen ob Typ wieder T
                            if (propertyAttribute != null! && property.CanWrite)
                            {
                                data.Properties.Add(new ConfigGeneratorProperty
                                {
                                    Label = propertyAttribute.Label,
                                    Property = property,
                                    SortIndex = propertyAttribute.SortIndex,
                                    PropertyAttribute = propertyAttribute
                                });
                            }
                        }

                        if (data.Properties.Count > 0)
                        {
                            Data.Add(data);
                        }
                    }
                }
            }
        }

        #region Properties

        /// <summary>
        ///     Daten des Generators
        /// </summary>
        public List<ConfigGeneratorObject> Data { get; } = new();

        #endregion

        /// <summary>
        ///     Daten übernehmen
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public object ProcessData(ConfigGeneratorObject o)
        {
            if (o == null!)
            {
                throw new ArgumentNullException(nameof(o));
            }

            var result = Activator.CreateInstance(o.ConfigType);
            foreach (var property in o.Properties)
            {
                var pt = property.Property.PropertyType;
                if (pt == null)
                {
                    continue;
                }

                if (pt == typeof(string))
                {
                    property.Property.SetValue(result, property.Value);
                }
                else if (pt == typeof(bool))
                {
                    property.Property.SetValue(result, Convert.ToBoolean(property.Value));
                }
                else if (pt == typeof(Int16))
                {
                    property.Property.SetValue(result, Convert.ToInt16(property.Value));
                }
                else if (pt == typeof(Int32))
                {
                    property.Property.SetValue(result, Convert.ToInt32(property.Value));
                }
                else if (pt == typeof(Int64))
                {
                    property.Property.SetValue(result, Convert.ToInt64(property.Value));
                }
                else if (pt == typeof(UInt16))
                {
                    property.Property.SetValue(result, Convert.ToUInt16(property.Value));
                }
                else if (pt == typeof(UInt32))
                {
                    property.Property.SetValue(result, Convert.ToUInt32(property.Value));
                }
                else if (pt == typeof(UInt64))
                {
                    property.Property.SetValue(result, Convert.ToUInt64(property.Value));
                }
                else if (pt == typeof(float))
                {
                    property.Property.SetValue(result, Convert.ToSingle(property.Value));
                }
                else if (pt == typeof(double))
                {
                    property.Property.SetValue(result, Convert.ToDouble(property.Value));
                }
                else if (pt == typeof(byte))
                {
                    property.Property.SetValue(result, Convert.ToByte(property.Value));
                }
                else if (pt == typeof(sbyte))
                {
                    property.Property.SetValue(result, Convert.ToSByte(property.Value));
                }
                else
                {
                    throw new ArgumentException($"Type {pt} not supported");
                }
            }

            return result;
        }

        /// <summary>
        ///     Leitet Typ von BissVsFileBase ab?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool CheckSearchedType(Type type)
        {
            if (type == null! || type.BaseType == null!)
            {
                return false;
            }

            if (type.IsAbstract)
            {
                return false;
            }

            if (type.GetInterface(typeof(T).Name) != null!)
            {
                return true;
            }

            if (type.BaseType.Name == typeof(T).Name)
            {
                return true;
            }

            if (type.BaseType.IsAbstract)
            {
                return CheckSearchedType(type.BaseType);
            }

            return false;
        }
    }
}