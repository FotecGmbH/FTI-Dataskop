// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BDA.Common.Exchange.Configs.Interfaces;
using Biss.Serialize;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Configs lokal laden / erzeugen</para>
    ///     Klasse ConfigHandler. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConfigHandler<T>
        where T : IConfigBase
    {
        private readonly FileInfo _localConfigFile;

        /// <summary>
        ///     Configs lokal laden / erzeugen
        /// </summary>
        /// <param name="localConfigFile"></param>
        public ConfigHandler(FileInfo localConfigFile)
        {
            _localConfigFile = localConfigFile;

            // ReSharper disable once UnusedVariable
            var s = typeof(T).Name;
        }

        #region Properties

        /// <summary>
        ///     Wurde die Konfiguration erzeugt oder gelesen
        /// </summary>
        public bool IsNewConfig { get; set; }

        /// <summary>
        ///     Existiert die Config lokal?
        /// </summary>
        public bool Exist => _localConfigFile.Exists;

        #endregion

        /// <summary>
        ///     Lokale Config Löschen
        /// </summary>
        public void Delete() => _localConfigFile.Delete();

        /// <summary>
        ///     Konfiguration lokal lesen
        /// </summary>
        /// <returns></returns>
        public T ReadConfig()
        {
            var fw = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (!_localConfigFile.Exists)
            {
                var c = Activator.CreateInstance<T>();
                c.Secret = Guid.NewGuid().ToString();
                c.ConfigVersion = -1;
                c.FirmwareVersion = fw;

                c.Name = typeof(T).Name.Replace("ex", "", StringComparison.InvariantCultureIgnoreCase).Replace("config", "", StringComparison.InvariantCultureIgnoreCase);
                var tmp = c.Secret.Split('-').ToList();
                if (tmp == null || tmp.Count == 0)
                {
                    throw new Exception("Invalid Secret Exeption!");
                }

                c.Name = $"{c.Name}-{tmp.Last()}";
                IsNewConfig = true;
                StoreConfig(c);
                return c;
            }

            var result = BissDeserialize.FromJson<T>(File.ReadAllText(_localConfigFile.FullName));
            if (!result.FirmwareVersion.Equals(fw, StringComparison.InvariantCultureIgnoreCase))
            {
                result.FirmwareVersion = fw;
                StoreConfig(result);
            }

            return result;
        }

        /// <summary>
        ///     Konfiguration lokal schreiben
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool StoreConfig(T config)
        {
            if (_localConfigFile.Exists)
            {
                _localConfigFile.Delete();
            }

            File.WriteAllText(_localConfigFile.FullName, config.ToJson());
            return true;
        }
    }
}