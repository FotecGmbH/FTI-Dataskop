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
using Azure.Storage.Blobs;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using WebExchange.Interfaces;

namespace BDA.Service.AppConnectivity.Helper
{
    /// <summary>
    ///     <para>Helper zur Dateiverwaltung im Azure Blobstore.</para>
    ///     Klasse AzureFileHelper. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class AzureFileHelper
    {
        private readonly IWebSettingsAzureFiles _webSettingsAzureFiles;

        /// <summary>
        ///     Helper zur Dateiverwaltung im Azure Blobstore.
        /// </summary>
        /// <param name="webSettingsAzureFiles">Settings for Azure files</param>
        public AzureFileHelper(IWebSettingsAzureFiles webSettingsAzureFiles)
        {
            _webSettingsAzureFiles = webSettingsAzureFiles;
        }

        /// <summary>
        ///     Saves or updates the given file in Azure blob storage using the blobname.
        /// </summary>
        /// <param name="blobName">The blobname for Azure.</param>
        /// <param name="file">The file to save.</param>
        /// <returns>The public link to the file at Azure storage.</returns>
        public async Task<string> SaveFile(string blobName, IEnumerable<byte> file)
        {
            try
            {
                if (file == null)
                {
                    throw new ArgumentNullException($"[{nameof(AzureFileHelper)}]({nameof(SaveFile)}): {nameof(file)}");
                }

                if (string.IsNullOrWhiteSpace(blobName))
                {
                    throw new ArgumentException($"[{nameof(AzureFileHelper)}]({nameof(SaveFile)}): {nameof(blobName)}");
                }

                var blobClient = new BlobClient(_webSettingsAzureFiles.BlobConnectionString, _webSettingsAzureFiles.BlobContainerName, blobName);

                // Upload the file
                var res = await blobClient.UploadAsync(new BinaryData(file.ToArray()), true).ConfigureAwait(false);

                if (res?.Value != null)
                {
                    if (_webSettingsAzureFiles.CdnLink.EndsWith("/"))
                    {
                        return _webSettingsAzureFiles.CdnLink +
                            _webSettingsAzureFiles.BlobContainerName + "/" +
                            blobName;
                    }

                    return _webSettingsAzureFiles.CdnLink + "/" +
                        _webSettingsAzureFiles.BlobContainerName + "/" +
                        blobName;
                }

                throw new ApplicationException($"[{nameof(AzureFileHelper)}]({nameof(SaveFile)}): Fehler beim CDN Upload!");
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(AzureFileHelper)}]({nameof(SaveFile)}): {e}");
                throw;
            }
        }

        /// <summary>
        ///     Check if a file with the given blobname exists in the Azure blob storage.
        /// </summary>
        /// <param name="blobName">The filename in the blob</param>
        /// <returns>True if blob exists, return false otherwise.</returns>
        public async Task<bool> ExistsFile(string blobName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobName))
                {
                    throw new ArgumentException($"[{nameof(AzureFileHelper)}]({nameof(ExistsFile)}): {nameof(blobName)}");
                }

                // Delete from Blob
                var blobClient = new BlobClient(_webSettingsAzureFiles.BlobConnectionString, _webSettingsAzureFiles.BlobContainerName, blobName);

                var res = await blobClient.ExistsAsync().ConfigureAwait(false);

                if (res?.Value is null)
                {
                    throw new ApplicationException($"[{nameof(AzureFileHelper)}]({nameof(ExistsFile)}): Could not delete File!");
                }

                return res.Value;
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(AzureFileHelper)}]({nameof(ExistsFile)}): {e}");
                throw;
            }
        }

        /// <summary>
        ///     Deletes a file with the given blobname from the Azure blob storage.
        /// </summary>
        /// <param name="blobName">The filename in the blob</param>
        /// <returns>True if blob exists and was deleted, return false otherwise.</returns>
        public async Task<bool> DeleteFile(string blobName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobName))
                {
                    throw new ArgumentException(nameof(blobName));
                }

                // Delete from Blob
                var blobClient = new BlobClient(_webSettingsAzureFiles.BlobConnectionString, _webSettingsAzureFiles.BlobContainerName, blobName);

                var res = await blobClient.DeleteIfExistsAsync().ConfigureAwait(false);

                if (res?.Value is null)
                {
                    throw new ApplicationException($"[{nameof(AzureFileHelper)}]({nameof(ExistsFile)}): Could not delete File!");
                }

                return res.Value;
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(AzureFileHelper)}]({nameof(ExistsFile)}): {e}");
                throw;
            }
        }
    }
}