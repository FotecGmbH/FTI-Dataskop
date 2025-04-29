// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Database;
using Database.Tables;
using Microsoft.Extensions.Logging;
using WebExchange;

namespace BDA.Service.AppConnectivity.Helper
{
    /// <summary>
    ///     <para>Dateimanipulation im Azure Blob-Storage und in der TableFiles</para>
    ///     Klasse FilesDbBlob. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class FilesDbBlob
    {
        /// <summary>
        ///     Datei im Azure Blob Storage speichern und in der TableFiles anlegen
        /// </summary>
        /// <param name="fileName">Oroginaldateiname</param>
        /// <param name="file">Daten der Datei</param>
        /// <returns></returns>
        public static async Task<DcTransferFileResult> StoreFile(string fileName, IReadOnlyCollection<byte> file)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var fileHelper = new AzureFileHelper(WebSettings.Current());

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "unknown";
            }

            var index = fileName.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
            if (index < 0)
            {
                index = fileName.LastIndexOf("\\", StringComparison.InvariantCultureIgnoreCase);
            }

            if (index >= 0)
            {
                fileName = fileName.Substring(index + 1);
            }

            index = fileName.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
            var suffix = "";
            if (index >= 0)
            {
                suffix = fileName.Substring(index);
            }

            var tblFile = new TableFile
            {
                PublicLink = string.Empty,
                BlobName = String.Empty,
                Name = fileName,
                AdditionalData = string.Empty,
                Bytes = null
            };
            db.TblFiles.Add(tblFile);
            await db.SaveChangesAsync().ConfigureAwait(true);
            var blobName = $"{tblFile.Id}{suffix}";
            try
            {
                var publicLink = await fileHelper.SaveFile(blobName, file).ConfigureAwait(true);
                tblFile.BlobName = blobName;
                tblFile.PublicLink = publicLink;
                await db.SaveChangesAsync().ConfigureAwait(true);
            }
            catch
            {
                db.TblFiles.Remove(tblFile);
                await db.SaveChangesAsync().ConfigureAwait(true);
                throw;
            }

            return new DcTransferFileResult
            {
                DbId = tblFile.Id,
                FileLink = tblFile.PublicLink,
                CommonData = string.Empty,
                StoreResult = new DcStoreResult(),
            };
        }

        /// <summary>
        ///     Datei löschen aus DB und aus Blob
        /// </summary>
        /// <returns></returns>
        public static async Task DeleteFile(Db db, long fileId)
        {
            if (db == null!)
            {
                throw new NullReferenceException($"[{nameof(FilesDbBlob)}]({nameof(DeleteFile)}): Database context is NULL!");
            }

            var tblFile = db.TblFiles.FirstOrDefault(x => x.Id == fileId);
            Logging.Log.LogTrace($"[{nameof(FilesDbBlob)}]({nameof(DeleteFile)}): Id {fileId}");
            if (tblFile == null)
            {
                throw new FileNotFoundException($"[{nameof(FilesDbBlob)}]({nameof(DeleteFile)}): File with Id {fileId} does not exist in database!");
            }

            var fileHelper = new AzureFileHelper(WebSettings.Current());

            if (await fileHelper.ExistsFile(tblFile.BlobName).ConfigureAwait(true))
            {
                var res = await fileHelper.DeleteFile(tblFile.BlobName).ConfigureAwait(true);

                if (!res)
                {
                    throw new ApplicationException($"[{nameof(FilesDbBlob)}]({nameof(DeleteFile)}): Could not Delete File whit the name {tblFile.BlobName} in Blob");
                }
            }

            db.TblFiles.Remove(tblFile);
            await db.SaveChangesAsync().ConfigureAwait(true);
        }
    }
}