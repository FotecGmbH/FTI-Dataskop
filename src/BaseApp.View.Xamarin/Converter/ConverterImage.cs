// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Biss.Apps.Model;
using Biss.Log.Producer;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using Xamarin.Forms;

namespace BaseApp.View.Xamarin.Converter
{
    /// <summary>
    ///     <para>Url to ImageSource converter.</para>
    ///     Klasse UrlToImageSourceConverter. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConverterImage : IValueConverter
    {
        private ImageSource GetUserImage()
        {
            var dc = VmProjectBase.GetVmBaseStatic.Dc;
            if (dc.CoreConnectionInfos != null! && dc.CoreConnectionInfos.UserOk)
            {
                if (dc.DcExUser.Data.HasImage)
                {
                    try
                    {
                        return ImageSource.FromUri(new Uri(dc.DcExUser.Data.UserImageLink));
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(ConverterImage)}]({nameof(GetUserImage)}): UserImageLink konnte nich verarbeitet werden. {e}");
                        return ImageSource.FromStream(() => Images.ReadImageAsStream(EnumEmbeddedImage.DefaultUserImage_png));
                    }
                }
            }

            return ImageSource.FromStream(() => Images.ReadImageAsStream(EnumEmbeddedImage.DefaultUserImage_png));
        }

        #region Interface Implementations

        /// <summary>
        ///     Konvertiert ein Objekt für XAML
        /// </summary>
        /// <param name="value">Wert zum konvertieren für das UI</param>
        /// <param name="targetType">Zieltyp des Werts</param>
        /// <param name="parameter">EnumEmbeddedImage</param>
        /// <param name="culture">Aktuelle Kultur</param>
        /// <returns>Konvertierter Wert oder null</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            #region Exfile

            if (value is ExFile file)
            {
                if (file.Bytes != null! && file.Bytes.Any())
                {
                    return ImageSource.FromStream(() => new MemoryStream(file.Bytes));
                }

                if (!string.IsNullOrWhiteSpace(file.DownloadLink))
                {
                    if (file.DownloadLink.ToUpperInvariant().StartsWith("HTTP"))
                    {
                        return ImageSource.FromUri(new Uri(file.DownloadLink));
                    }

                    return ImageSource.FromFile(file.DownloadLink);
                }
            }

            #endregion

            #region byte[] und Stream

            if (value is byte[] bytes && bytes.Any())
            {
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }

            if (value is Stream stream && stream.Length > 0)
            {
                return ImageSource.FromStream(() => stream);
            }

            #endregion

            #region URL (string)

            if (value is string url)
            {
#pragma warning disable CA1508 // Avoid dead conditional code
                if (!string.IsNullOrWhiteSpace(url))
#pragma warning restore CA1508 // Avoid dead conditional code
                {
                    if (url.ToUpperInvariant().StartsWith("HTTP"))
                    {
                        return ImageSource.FromUri(new Uri(url));
                    }

                    return ImageSource.FromFile(url);
                }
            }

            #endregion

            #region EnumEmbeddedImage

            if (parameter != null!)
            {
                if (parameter is EnumEmbeddedImage embedded)
                {
                    return ImageSource.FromStream(() => Images.ReadImageAsStream(embedded));
                }


                if (parameter is string defaultImage && defaultImage.Equals("DefaultUserImage", StringComparison.Ordinal))
                {
                    // Workaround
                    return GetUserImage();
                }


                return null!;
            }

            #endregion

            return null!;
        }

        /// <summary>
        ///     Konvertiert ein Objekt von XAML
        /// </summary>
        /// <param name="value">Wert zum konvertieren für das Datenobjekt</param>
        /// <param name="targetType">Zieltyp des Werts</param>
        /// <param name="parameter">Zusätzlicher Parameter aus XAML</param>
        /// <param name="culture">Aktuelle Kultur</param>
        /// <returns>Konvertierter Wert oder UnsetValue</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null!;
        }

        #endregion
    }
}