// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using BDA.Common.Exchange.Model.ConfigApp;

namespace BDA.Service.Com.NewValueNotification
{
    public interface INewValueNotificationService
    {
        Task SendNewValueNotificationAsync(long measurementDefinitionId, ExMeasurement newValue);
    }
}