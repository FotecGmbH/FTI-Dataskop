// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biss.Interfaces;

namespace BDA.Common.Exchange.GatewayService
{
    /// <summary>
    ///     <para>Downlink nachricht inklusive id des geraetes welches diese empfangen soll</para>
    ///     Klasse ExDownlinkMessageForDevice. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExDownlinkMessageForDevice : IBissModel
    {
        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="iotDeviceId">geraet id</param>
        /// <param name="message">nachricht</param>
#pragma warning disable CS8618, CS9264
        public ExDownlinkMessageForDevice(long iotDeviceId, byte[] message)
#pragma warning restore CS8618, CS9264
        {
            IotDeviceId = iotDeviceId;
            Message = message;
        }

        #region Properties

        /// <summary>
        ///     Db Id des geraetes welches die nachricht empfangen soll
        /// </summary>
        public long IotDeviceId { get; set; }

        /// <summary>
        ///     Nachricht
        /// </summary>
        public byte[] Message { get; set; }

        #endregion

        /// <summary>
        ///     On property changed
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Interface Implementations

        /// <summary>
        ///     property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}