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
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Log.Producer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BDA.Common.ParserCompiler
{
    /// <summary>
    ///     Statische Klasse welche die Funktionen zum Kompilieren des UserParserCodes beinhaltet.
    /// </summary>
    public static class Compiler
    {
        /// <summary>
        ///     Kompiliert ein Assembly aus dem übergebenen usercode.
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<MemoryStream> GetAssembly(string userCode)
        {
            var text = userCode;
            var template = await File.ReadAllTextAsync("DataConverterTemplate.cs").ConfigureAwait(false);
            var code = template.Replace("// <Usercode>", text, StringComparison.InvariantCultureIgnoreCase);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var assemblyName = Path.GetRandomFileName();
            var refPaths = new[]
            {
                typeof(Object).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                typeof(JsonConvert).GetTypeInfo().Assembly.Location,
                typeof(Enumerable).GetTypeInfo().Assembly.Location,
                typeof(ExValue).GetTypeInfo().Assembly.Location,
                Assembly.Load("netstandard, Version=2.1.0.0, Culture=neutral").Location,
                typeof(EnumValueTypes).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")
            };
            // ReSharper disable once CoVariantArrayConversion
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] {syntaxTree},
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                Logging.Log.LogWarning($"[{nameof(Compiler)}]({nameof(GetAssembly)}): Compilation failed");

                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                var error = new StringBuilder("");

                foreach (var diagnostic in failures)
                {
                    error.AppendLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    Logging.Log.LogWarning($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }

                throw new InvalidOperationException(error.ToString());
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}