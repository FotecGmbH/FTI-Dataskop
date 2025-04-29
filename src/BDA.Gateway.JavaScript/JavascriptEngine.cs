// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using JavaScriptEngineSwitcher.Jint;

namespace BDA.Gateway.JavaScript
{
    /// <summary>
    /// JavascriptEngine
    /// </summary>
    public static class JavascriptEngine
    {
        /// <summary>
        ///       Decode
        /// </summary>
        public static void Decode()
        {
            var engine = new JintJsEngineFactory().CreateEngine();
            engine.ExecuteFile("Test.js");
            // ReSharper disable once UnusedVariable
            var result = engine.Evaluate<string>("JSON.stringify(Decoder([1,2,3,4], 5));");
        }
    }
}