// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Microsoft.Extensions.DependencyInjection;

namespace BDA.Service.Com.NewValueNotification
{
    /// <summary>
    ///     <para>NewValueNotificationService Extensions</para>
    ///     Klasse NewValueNotificationServiceExtensions. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class NewValueNotificationServiceExtensions
    {
        public static void AddNewValueNotificationService(this IServiceCollection services)
        {
            services.AddSingleton<INewValueNotificationService, NewValueNotificationService>();
        }
    }
}