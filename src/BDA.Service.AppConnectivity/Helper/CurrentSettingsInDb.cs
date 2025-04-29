// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model;
using Biss.Common;
using Biss.Log.Producer;
using Database;
using Microsoft.Extensions.Logging;

namespace BDA.Service.AppConnectivity.Helper
{
    /// <summary>
    ///     <para>Aktuelle Einstellungen in der Datenbank</para>
    ///     Klasse CurrentSettingsInDb. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class CurrentSettingsInDb
    {
        private static ExSettingsInDb _current = null!;
        private static readonly object ModifyLock = new object();

        #region Properties

        /// <summary>
        ///     Aktuelle Einstellungen in Db
        /// </summary>
        public static ExSettingsInDb Current
        {
            get
            {
                if (_current == null!)
                {
                    ReadOrUpdateCurrent();
                }

                return _current!;
            }
        }

        #endregion

        /// <summary>
        ///     Setting Element in Db aktualisieren
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="db"></param>
        public static void UpdateElement(EnumDbSettings key, string value, Db db)
        {
            if (db == null!)
            {
                Logging.Log.LogTrace($"[{nameof(CurrentSettingsInDb)}]({nameof(UpdateElement)}): {nameof(Db)} is NULL!");
                return;
            }

            var data = db.TblSettings.FirstOrDefault(e => e.Key == key);
            if (data == null)
            {
                Logging.Log.LogTrace($"[{nameof(CurrentSettingsInDb)}]({nameof(UpdateElement)}): Key {key} not found.");
                return;
            }

            lock (ModifyLock)
            {
                data.Value = value;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(CurrentSettingsInDb)}]({nameof(UpdateElement)}):{e}");
                    Logging.Log.LogTrace($"[{nameof(CurrentSettingsInDb)}]({nameof(UpdateElement)}): SaveChanges Error: {e}");
                    return;
                }
            }

            ReadOrUpdateCurrent();
        }

        /// <summary>
        ///     Current neu aus DB lesen
        /// </summary>
        private static void ReadOrUpdateCurrent()
        {
            lock (ModifyLock)
            {
                using var db = new Db();
                _current = new ExSettingsInDb();
                var settings = EnumUtil.GetValues<EnumDbSettings>();
                foreach (var setting in settings)
                {
                    var data = db.TblSettings.First(f => f.Key == setting).Value;
                    switch (setting)
                    {
                        case EnumDbSettings.Agb:
                            _current.AgbString = data;
                            break;
                        case EnumDbSettings.CurrentAppVersion:
                            _current.CurrentAppVersionString = data;
                            break;
                        case EnumDbSettings.MinAppVersion:
                            _current.MinAppVersionString = data;
                            break;
                        case EnumDbSettings.CommonMessage:
                            _current.CommonMessage = data;
                            break;
                        default:
                            Logging.Log.LogTrace($"[{nameof(CurrentSettingsInDb)}]({nameof(ReadOrUpdateCurrent)}): Key {setting} not found.");
                            break;
                    }
                }
            }
        }
    }
}