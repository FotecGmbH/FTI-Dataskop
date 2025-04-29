// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Dc.Core;
using Database;
using Database.Converter;
using Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.AppConnectivity.DataConnector
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse DcExDataconverter. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        #region Interface Implementations

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
        public async Task<List<DcServerListItem<ExDataconverter>>> GetDcExDataconverters(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
        {
            var result = new List<DcServerListItem<ExDataconverter>>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            //Gastbenutzer
            if (userId <= 0)
            {
                return result;
            }

            var converters = await db.TblDataconverters.ToListAsync().ConfigureAwait(false);
            foreach (var c in converters)
            {
                result.Add(new DcServerListItem<ExDataconverter>
                    {
                        Data = c.ToExDataconverter(),
                        Index = c.Id,
                        SortIndex = c.Id
                    }
                );
            }

            return result;
        }

        /// <summary>
        ///     Device will Listen Daten für DcIotDevices sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcListStoreResult> StoreDcExDataconverters(long deviceId, long userId, List<DcStoreListItem<ExDataconverter>> data, long secondId)
        {
            if (data == null!)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var r = new DcListStoreResult
            {
                SecondId = secondId,
                StoreResult = new(),
                ElementsStored = new()
            };

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            // ReSharper disable once RedundantAssignment
            TableDataconverter c = null!;

            foreach (var d in data)
            {
                var tmp = new DcListStoreResultIndexAndData();
                switch (d.State)
                {
                    case EnumDcListElementState.New:
                        c = new TableDataconverter();
                        tmp.BeforeStoreIndex = d.Index;
                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Modified:
                        c = db.TblDataconverters.First(f => f.Id == d.Index);
                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Deleted:
                        c = db.TblDataconverters.Where(f => f.Id == d.Index).Include(i => i.Templates).First();
                        break;
                    case EnumDcListElementState.None:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (d.State == EnumDcListElementState.Deleted)
                {
                    // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
                    db.TblMeasurementDefinitionTemplates.RemoveRange(c.Templates);
                    db.TblDataconverters.Remove(c);
                }
                else
                {
                    d.Data.ToTableDataconverter(c);
                }

                if (d.State == EnumDcListElementState.New)
                {
                    db.TblDataconverters.Add(c);
                }

                await db.SaveChangesAsync().ConfigureAwait(true);
                if (d.State == EnumDcListElementState.New)
                {
                    tmp.NewIndex = c.Id;
                    tmp.NewSortIndex = c.Id;
                    r.NewIndex.Add(tmp);
                }
            }

            return r;
        }

        /// <summary>
        ///     Daten Synchronisieren für DcExDataconverters
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        public Task<DcListSyncResultData<ExDataconverter>> SyncDcExDataconverters(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Daten Synchronisieren für DcExProjects
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        public Task<DcListSyncResultData<ExProject>> SyncDcExProjects(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}