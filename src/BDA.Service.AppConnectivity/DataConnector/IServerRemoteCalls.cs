// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Threading.Tasks;
using BDA.Common.Exchange.Configs.NewValueNotifications;
using BDA.Common.Exchange.Model;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Dc.Core;
using Biss.Dc.Server;
// ReSharper disable once RedundantUsingDirective
using System.Collections.Generic;

namespace BDA.Service.AppConnectivity.DataConnector
{
    /// <summary>
    ///     Diese Funktionen müssen am Server implementiert werden
    /// </summary>
    public interface IServerRemoteCalls : IDcCoreRemoteCalls
    {
        #region DcExDeviceInfo

        /// <summary>
        ///     Device fordert Daten für DcExDeviceInfo
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        Task<ExDeviceInfo> GetDcExDeviceInfo(long deviceId, long userId);

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcStoreResult> SetDcExDeviceInfo(long deviceId, long userId, ExDeviceInfo data);

        #endregion

        #region DcExUser

        /// <summary>
        ///     Device fordert Daten für DcExUser
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        Task<ExUser> GetDcExUser(long deviceId, long userId);

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcStoreResult> SetDcExUser(long deviceId, long userId, ExUser data);

        #endregion

        #region DcExUserPassword

        /// <summary>
        ///     Device fordert Daten für DcExUserPassword
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        Task<ExUserPassword> GetDcExUserPassword(long deviceId, long userId);

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcStoreResult> SetDcExUserPassword(long deviceId, long userId, ExUserPassword data);

        #endregion

        #region DcExLocalAppData

        /// <summary>
        ///     Device fordert Daten für DcExLocalAppData
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        Task<ExLocalAppSettings> GetDcExLocalAppData(long deviceId, long userId);

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcStoreResult> SetDcExLocalAppData(long deviceId, long userId, ExLocalAppSettings data);

        #endregion

        #region DcExSettingsInDb

        /// <summary>
        ///     Device fordert Daten für DcExSettingsInDb
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        Task<ExSettingsInDb> GetDcExSettingsInDb(long deviceId, long userId);

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcStoreResult> SetDcExSettingsInDb(long deviceId, long userId, ExSettingsInDb data);

        #endregion

        #region DcExCompanies

        /// <summary>
        ///     Device fordert Listen Daten für DcExCompanies
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
        /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <param name="filter">Optionaler Filter für die Daten</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        [Obsolete]
        Task<List<DcServerListItem<ExCompany>>> GetDcExCompanies(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExCompanies sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExCompanies(long deviceId, long userId, List<DcStoreListItem<ExCompany>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExCompanies
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExCompany>> SyncDcExCompanies(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion

        #region DcExCompanyUsers

        /// <summary>
        ///     Device fordert Listen Daten für DcExCompanyUsers
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
        /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <param name="filter">Optionaler Filter für die Daten</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        [Obsolete]
        Task<List<DcServerListItem<ExCompanyUser>>> GetDcExCompanyUsers(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExCompanyUsers sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExCompanyUsers(long deviceId, long userId, List<DcStoreListItem<ExCompanyUser>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExCompanyUsers
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExCompanyUser>> SyncDcExCompanyUsers(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion

        #region DcExGateways

        /// <summary>
        ///     Device fordert Listen Daten für DcExGateways
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
        /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <param name="filter">Optionaler Filter für die Daten</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        [Obsolete]
        Task<List<DcServerListItem<ExGateway>>> GetDcExGateways(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExGateways sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExGateways(long deviceId, long userId, List<DcStoreListItem<ExGateway>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExGateways
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExGateway>> SyncDcExGateways(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion

        #region DcExIotDevices

        /// <summary>
        ///     Device fordert Listen Daten für DcExIotDevices
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
        /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <param name="filter">Optionaler Filter für die Daten</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        [Obsolete]
        Task<List<DcServerListItem<ExIotDevice>>> GetDcExIotDevices(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExIotDevices sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExIotDevices(long deviceId, long userId, List<DcStoreListItem<ExIotDevice>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExIotDevices
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExIotDevice>> SyncDcExIotDevices(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion

        #region DcExMeasurementDefinition

        /// <summary>
        ///     Device fordert Listen Daten für DcExMeasurementDefinition
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
        /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <param name="filter">Optionaler Filter für die Daten</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        [Obsolete]
        Task<List<DcServerListItem<ExMeasurementDefinition>>> GetDcExMeasurementDefinition(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExMeasurementDefinition sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExMeasurementDefinition(long deviceId, long userId, List<DcStoreListItem<ExMeasurementDefinition>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExMeasurementDefinition
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExMeasurementDefinition>> SyncDcExMeasurementDefinition(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion

        #region DcExDataconverters

        /// <summary>
        ///     Device fordert Listen Daten für DcExDataconverters
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
        /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <param name="filter">Optionaler Filter für die Daten</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        [Obsolete]
        Task<List<DcServerListItem<ExDataconverter>>> GetDcExDataconverters(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExDataconverters sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExDataconverters(long deviceId, long userId, List<DcStoreListItem<ExDataconverter>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExDataconverters
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExDataconverter>> SyncDcExDataconverters(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion

        #region DcExGlobalConfig

        /// <summary>
        ///     Device fordert Listen Daten für DcExGlobalConfig
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
        /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <param name="filter">Optionaler Filter für die Daten</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        [Obsolete]
        Task<List<DcServerListItem<ExGlobalConfig>>> GetDcExGlobalConfig(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExGlobalConfig sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExGlobalConfig(long deviceId, long userId, List<DcStoreListItem<ExGlobalConfig>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExGlobalConfig
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExGlobalConfig>> SyncDcExGlobalConfig(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion

        #region DcExProjects

        /// <summary>
        ///     Device fordert Listen Daten für DcExProjects
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
        /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <param name="filter">Optionaler Filter für die Daten</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        [Obsolete]
        Task<List<DcServerListItem<ExProject>>> GetDcExProjects(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExProjects sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExProjects(long deviceId, long userId, List<DcStoreListItem<ExProject>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExProjects
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExProject>> SyncDcExProjects(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion

        #region DcExNewValueNotifications

        /// <summary>
        ///     Device fordert Listen Daten für DcExNewValueNotifications
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
        /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <param name="filter">Optionaler Filter für die Daten</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        [Obsolete]
        Task<List<DcServerListItem<ExNewValueNotification>>> GetDcExNewValueNotifications(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExNewValueNotifications sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExNewValueNotifications(long deviceId, long userId, List<DcStoreListItem<ExNewValueNotification>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExNewValueNotifications
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExNewValueNotification>> SyncDcExNewValueNotifications(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion
    }
}