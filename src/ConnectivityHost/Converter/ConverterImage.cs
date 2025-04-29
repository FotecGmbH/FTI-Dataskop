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
using Biss.Apps.Model;
using Exchange.Resources;

namespace ConnectivityHost.Converter
{
    /// <summary>
    ///     <para>Bilder für die Darstellung in HTML umwandeln</para>
    ///     Klasse ConverterImage. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ConverterImage
    {
        #region EmbeddedImage

        /// <summary>
        ///     Eingebettetes Bild in base64 umwandeln.
        /// </summary>
        /// <param name="embeddedImage">Eingebettetes Bild</param>
        /// <returns></returns>
        public static string Convert(EnumEmbeddedImage embeddedImage)
        {
            var img = Images.ReadImageAsStream(embeddedImage);

            using MemoryStream ms = new();
            img.CopyTo(ms);

            return Convert(ms.ToArray());
        }

        #endregion

        #region ExFile

        /// <summary>
        ///     Convert Image von Exfile
        /// </summary>
        /// <param name="file">File</param>
        /// <returns>Image</returns>
        public static string Convert(ExFile? file)
        {
            if (!string.IsNullOrWhiteSpace(file?.DownloadLink))
            {
                return file.DownloadLink;
            }

            if (file?.Bytes != null && file.Bytes.Any())
            {
                return Convert(file.Bytes);
            }

            return string.Empty;
        }

        #endregion

        #region Byte[] und Stream

        /// <summary>
        ///     Convert Image aus Stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Image</returns>
        public static string Convert(Stream? stream)
        {
            if (stream == null)
            {
                return string.Empty;
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return Convert(ms.ToArray());
        }

        /// <summary>
        ///     Convert Image aus Byte Array
        /// </summary>
        /// <param name="bytes">Byte Array</param>
        /// <returns>Image</returns>
        public static string Convert(IEnumerable<byte> bytes)
        {
            return $"data:image/png;base64,{System.Convert.ToBase64String(bytes.ToArray())}";
        }

        #endregion
    }
}