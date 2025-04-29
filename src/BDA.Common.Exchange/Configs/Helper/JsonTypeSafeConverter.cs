// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using BDA.Common.Exchange.Configs.NewValueNotifications;
using BDA.Common.Exchange.Configs.NewValueNotifications.NewValueNotificationRules;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BDA.Common.Exchange.Configs.Helper
{
    /// <summary>
    ///     <para>Hilfsklasse zum (de)serialisieren</para>
    ///     Klasse JsonTypeSafeConverter. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class JsonTypeSafeConverter
    {
        private readonly JsonSerializerSettings _defaultJsonSerializerSettings = new JsonSerializerSettings
        {
#pragma warning disable CA2326 // Do not use TypeNameHandling values other than None
            TypeNameHandling = TypeNameHandling.Objects,
#pragma warning restore CA2326 // Do not use TypeNameHandling values other than None
            SerializationBinder = new JsonTypeSafeSerializationBinder
            {
                KnownTypes = new List<Type>
                {
                    typeof(ExNewValueNotificationBase),
                    typeof(ExNewValueNotificationWebHook),
                    typeof(ExNewValueNotificationMqtt),
                    typeof(ExNewValueNotificationEmail),
                    typeof(ExNewValueNotificationRuleBase),
                    typeof(ExNewNotificationRuleCompareNumbers),
                    typeof(ExNewNotificationRuleCompareString),
                    typeof(ExNewNotificationRuleCompareBool),
                    typeof(ExCompareDoubleValue),
                    typeof(ExCompareStringValue)
                }
            },
        };

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        ///     Hilfsklasse zum deserialisieren
        /// </summary>
        /// <param name="jsonSerializerSettings"></param>
        public JsonTypeSafeConverter(JsonSerializerSettings? jsonSerializerSettings = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings ?? _defaultJsonSerializerSettings;
        }

        /// <summary>
        ///     Serialisieren
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Serialize(object? data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(data, Formatting.Indented, _jsonSerializerSettings);
        }

        /// <summary>
        ///     Deserialisieren
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T? Deserialize<T>(string data) where T : class
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(data, _jsonSerializerSettings);
        }
    }

    /// <summary>
    ///     SerializationBinder
    /// </summary>
    public class JsonTypeSafeSerializationBinder : ISerializationBinder
    {
        #region Properties

        /// <summary>
        ///     Typen die bekannt sind
        /// </summary>
        public IList<Type> KnownTypes { get; set; } = new List<Type>();

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     When implemented, controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object</param>
        /// <returns>The type of the object the formatter creates a new instance of.</returns>
        public Type BindToType(string? assemblyName, string typeName)
        {
            return KnownTypes.FirstOrDefault(t => t.Name == typeName);
        }

        /// <summary>
        ///     When implemented, controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        #endregion
    }
}