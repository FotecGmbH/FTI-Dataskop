// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Concurrent;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BDA.Gateway.Com.Drei
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse LoggerRedirectCustomLogger. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class LoggerRedirectCustomLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private readonly IDisposable? _onChangeToken;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        #region Interface Implementations

        /// <summary>
        /// CreateLogger
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => Logging.LoggerFactory.CreateLogger(name));

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }

        #endregion
    }
}