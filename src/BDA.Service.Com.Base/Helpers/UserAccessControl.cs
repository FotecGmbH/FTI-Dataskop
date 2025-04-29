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
using BDA.Common.Exchange.Model;
using Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BDA.Service.Com.Base.Helpers
{
    /// <summary>
    ///     <para>Zugrifssberechtigung</para>
    ///     Klasse AccessControl. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class UserAccessControl
    {
        /// <summary>
        ///     ID der öffentlichen Firma
        /// </summary>
        public static int PublicCompanyId = 2;

        /// <summary>
        ///     cache fuer die Benutzerberechtigungen auf MeasurementDefinitions
        /// </summary>
        private static readonly Dictionary<long, List<UserPermission>> _cacheUserPermission = new Dictionary<long, List<UserPermission>>();

        #region Nested Types

        /// <summary>
        ///     Benutzer Berechtigung auf MeasurementDefinition
        /// </summary>
        private record UserPermission
        {
            #region Properties

            /// <summary>
            ///     MeasurementDefinitionId
            /// </summary>
            public required long MeasurementDefinitionId { get; set; }

            /// <summary>
            ///     Gueltig bis
            /// </summary>
            public required DateTime ValidUntil { get; set; }

            #endregion
        }

        #endregion

        /// <summary>
        ///     Berechtigungsabfrage
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="readAndWrite">Les- und Schreibrechte</param>
        /// <param name="user">Benutzer</param>
        /// <param name="db">DB Kontext</param>
        /// <returns>Berechtigt oder nicht berechtigt</returns>
        public static async Task<bool> HasMeasurmentResultPermission(ExUser user, Db db, long id, bool readAndWrite = false)
        {
            if (db == null || user == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (!user.IsAdmin)
            {
                var tblMeasuremetResult = await db.TblMeasurementResults.FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(true);
                if (tblMeasuremetResult == null)
                {
                    return false;
                }

                var measurmentDefinitionId = tblMeasuremetResult.TblMeasurementDefinitionId;

                return await HasMeasurmentDefinitionPermission(user, db, measurmentDefinitionId, readAndWrite).ConfigureAwait(true);
            }

            return true;
        }

        /// <summary>
        ///     Berechtigungsabfrage
        /// </summary>
        /// <param name="user">Benutzer</param>
        /// <param name="db">DB Kontext</param>
        /// <param name="id">Id</param>
        /// <param name="readAndWrite">Les- und Schreibrechte</param>
        /// <returns></returns>
        public static async Task<bool> HasMeasurmentDefinitionPermission(ExUser user, Db db, long id, bool readAndWrite = false)
        {
            if (db == null || user == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (!user.IsAdmin)
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                var hasPermissions = _cacheUserPermission.TryGetValue(user.Id, out List<UserPermission> permissions);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                if (hasPermissions)
                {
                    var perm = permissions!.Find(mdId => mdId.MeasurementDefinitionId == id);
                    if (perm != null && perm.ValidUntil > DateTime.Now)
                    {
                        perm.ValidUntil = DateTime.Now.AddMinutes(5);
                        return true;
                    }
                }

                var tblMtoPAssignment = await db.TblMeasurementDefinitionToProjectAssignments.FirstOrDefaultAsync(a => a.TblMeasurementDefinitionId == id).ConfigureAwait(true);
                if (tblMtoPAssignment == null)
                {
                    return false;
                }

                var projectId = tblMtoPAssignment.TblProjctId;

                var result = await HasProjectPermission(user, db, projectId!.Value, readAndWrite).ConfigureAwait(true);

                if (result) // User darf auf die Measurementdefinition zugreifen
                {
                    if (!hasPermissions)
                    {
                        _cacheUserPermission.Add(user.Id, new List<UserPermission>
                        {
                            new UserPermission {MeasurementDefinitionId = id, ValidUntil = DateTime.Now.AddMinutes(5)}
                        });
                    }
                    else
                    {
                        var perm = permissions!.Find(mdId => mdId.MeasurementDefinitionId == id);
                        if (perm == null)
                        {
                            permissions.Add(new UserPermission {MeasurementDefinitionId = id, ValidUntil = DateTime.Now.AddMinutes(5)});
                        }
                        else
                        {
                            // ReSharper disable once RedundantSuppressNullableWarningExpression
                            perm!.ValidUntil = DateTime.Now.AddMinutes(5);
                        }
                    }
                }

                CleanUpCache(); // wird nicht bei jedem Aufruf ausgefuehrt(fruehere returns), sollte aber trotzdem ausreichend oft gelehrt werden.

                return result;
            }

            return true;
        }


        /// <summary>
        ///     Berechtigungsabfrage
        /// </summary>
        /// <param name="user">Benutzer</param>
        /// <param name="db">DB Kontext</param>
        /// <param name="projectId">Projekt ID</param>
        /// <param name="readAndWrite">Les- und Schreibrechte</param>
        /// <returns></returns>
        public static async Task<bool> HasProjectPermission(ExUser user, Db db, long projectId, bool readAndWrite = false)
        {
            if (db == null)
            {
                throw new ArgumentException(null, nameof(db));
            }

            if (user == null)
            {
                throw new ArgumentException(null, nameof(user));
            }

            if (!user.IsAdmin)
            {
                var tblProject = await db.TblProjects.AsNoTracking().FirstOrDefaultAsync(a => a.Id == projectId).ConfigureAwait(true);
                if (tblProject == null)
                {
                    return false;
                }

                var companyId = tblProject.TblCompanyId;

                if (readAndWrite)
                {
                    if (!user.IsAdmInCompany(companyId) && !user.CanUserWriteInCompany(companyId))
                    {
                        return false;
                    }
                }
                else
                {
                    //Zugriff auf öffentliche Firma
                    if (companyId == PublicCompanyId)
                    {
                        return true;
                    }

                    if (!user.IsAdmInCompany(companyId) && !user.CanUserReadInCompany(companyId) && !user.CanUserWriteInCompany(companyId))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Nicht berechtigt
        /// </summary>
        /// <returns>Nicht berechtigt</returns>
        public static JsonResult Unauthorized() => new(new {message = "Unauthorized"}) {StatusCode = StatusCodes.Status401Unauthorized};

        /// <summary>
        ///     Methode welche alle Berechtigungen aus dem Cache löscht welche abgelaufen sind
        /// </summary>
        private static void CleanUpCache()
        {
            foreach (var user in _cacheUserPermission)
            {
                user.Value.RemoveAll(a => a.ValidUntil < DateTime.Now);
            }

            var entriesToRemove = _cacheUserPermission.Where(perm => perm.Value.Count == 0).Select(perm => perm.Key);

            foreach (var key in entriesToRemove)
            {
                _cacheUserPermission.Remove(key);
            }
        }
    }
}