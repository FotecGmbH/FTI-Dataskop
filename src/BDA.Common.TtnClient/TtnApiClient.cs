// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using BDA.Common.Exchange.Configs.Enums;
using BDA.Common.Exchange.Configs.GlobalConfigs;
using BDA.Common.Exchange.Configs.Upstream.Ttn;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BDA.Common.TtnClient
{
    /// <summary>
    ///     <para>Der Client um mit TTN zu interagieren</para>
    ///     Klasse TtnApiClient. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class TtnApiClient
    {
        private readonly string _cliConfigPath;
        private readonly string _cliPath;
        private GcTtn _config;
        private bool _initialized;

        public TtnApiClient(GcTtn config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _cliPath = @"ttnclitool\ttn-lw-cli.exe";
            _cliConfigPath = @"ttnclitool\.ttn-lw-cli.yml";
        }

        public async Task ChangeAccount(GcTtn config)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (config is null || _config.ApiKey == config.ApiKey)
            {
                return;
            }

            _config = config;

            // Konfiguration des cli tools / Login
            if (!_initialized)
            {
                await Init().ConfigureAwait(false);
            }

            var error = new List<string>();
            var loginresult = await RunCliCommand($"login --api-key {_config.ApiKey}", error: error).ConfigureAwait(false);

            if (loginresult != 0)
            {
                var apikey = "";
                if (_config.ApiKey.Length > 6)
                {
                    apikey = _config.ApiKey.Substring(_config.ApiKey.Length - 5);
                }

                Logging.Log.LogError($"Error while logging in to ttn using apikey ending with: \"{apikey}\". {Environment.NewLine}Error: {string.Join(Environment.NewLine, error)}");
                throw new InvalidOperationException(GetTtnError(error));
            }

            Logging.Log.LogInfo($"[{nameof(TtnApiClient)}]({nameof(Init)}): TTN-Apiclient successfully changedAccount");
        }

        /// <summary>
        ///     Führt 2 Befehle aus: "use eu1.cloud.thethings.network" und "login"
        /// </summary>
        /// <returns></returns>
        public async Task Init()
        {
            if (File.Exists(_cliConfigPath))
            {
                File.Delete(_cliConfigPath);
            }

            var error = new List<string>();

            await RunCliCommand($"use {_config.Zone}").ConfigureAwait(false);
            var loginresult = await RunCliCommand($"login --api-key {_config.ApiKey}", error: error).ConfigureAwait(false);

            if (loginresult != 0)
            {
                var apikey = "";
                if (_config.ApiKey.Length > 6)
                {
                    apikey = _config.ApiKey.Substring(_config.ApiKey.Length - 5);
                }

                Logging.Log.LogError($"Error while logging in to ttn using apikey ending with: \"{apikey}\". {Environment.NewLine}Error: {string.Join(Environment.NewLine, error)}");
                throw new InvalidOperationException(GetTtnError(error));
            }

            Logging.Log.LogInfo($"[{nameof(TtnApiClient)}]({nameof(Init)}): TTN-Apiclient successfully initialized");

            _initialized = true;
        }

        public async Task<string?> GetDevaddr(string deviceid, string appid)
        {
            // Konfiguration des cli tools / Login
            if (!_initialized)
            {
                await Init().ConfigureAwait(false);
            }

            Logging.Log.LogInfo($"[{nameof(TtnApiClient)}]({nameof(GetAllApplications)}): Getting deviceaddr of Device-Id: {deviceid}");

            // Process starten und error/output lesen
            var output = new List<string>();
            var error = new List<string>();
            var resultcode = await RunCliCommand($"end-devices get {appid} {deviceid} --pending-mac-state.queued-join-accept.dev-addr", output, error).ConfigureAwait(false);

            // Fehler bei der Abfrage => Abbruch
            if (resultcode != 0)
            {
                Logging.Log.LogError($"Error while requesting Devices from ttn.{Environment.NewLine}Error: {string.Join(Environment.NewLine, error)}");
                throw new InvalidOperationException(GetTtnError(error));
            }

            // Result ist eine Liste von Endgeräten. Extrahiere die device_id
            var device = JObject.Parse(string.Join("", output));
            return (string?) device["ids"]?["dev_addr"];
        }

        public async Task<bool> ApplicationExists(string appid)
        {
            var apps = await GetAllApplications().ConfigureAwait(false);
            return apps.Contains(appid);
        }

        public async Task<List<string>> GetAllApplications()
        {
            // Konfiguration des cli tools / Login
            if (!_initialized)
            {
                await Init().ConfigureAwait(false);
            }

            Logging.Log.LogInfo($"[{nameof(TtnApiClient)}]({nameof(GetAllApplications)}): Getting all TTN-Applikations");

            // Process starten und error/output lesen
            var output = new List<string>();
            var error = new List<string>();
            var resultcode = await RunCliCommand("applications list ", output, error).ConfigureAwait(false);

            // Fehler bei der Abfrage => Abbruch
            if (resultcode != 0)
            {
                Logging.Log.LogError($"Error while requesting Devices from ttn.{Environment.NewLine}Error: {string.Join(Environment.NewLine, error)}");
                throw new InvalidOperationException(GetTtnError(error));
            }

            // Result ist eine Liste von Endgeräten. Extrahiere die device_id
            var devices = JArray.Parse(string.Join("", output));
            return devices.Select(t => (string) t["ids"]?["application_id"]!).ToList();
        }


        /// <summary>
        ///     Erzeugt eine neue Application auf TTN.
        /// </summary>
        /// <param name="appid">Name der Application</param>
        /// <param name="userid">Username (bsp: fotec)</param>
        /// <returns>Returned den erzeugten APIKey</returns>
        /// <exception cref="InvalidOperationException">Fehler beim Erstellen aufgetreten siehe Logs</exception>
        public async Task<string> CreateApplication(string appid, string userid)
        {
            // Konfiguration des cli tools / Login
            if (!_initialized)
            {
                await Init().ConfigureAwait(false);
            }

            Logging.Log.LogInfo($"[{nameof(TtnApiClient)}]({nameof(CreateApplication)}): Creating TTN-Application with AppID: {appid}, UserID: {userid}");

            // Process starten und error/output lesen
            var output = new List<string>();
            var error = new List<string>();
            var resultcode = await RunCliCommand($"application create {appid} --user-id {userid} ", output, error).ConfigureAwait(false);

            // Fehler bei der Abfrage => Abbruch
            if (resultcode != 0)
            {
                Logging.Log.LogError($"Error while creating application on ttn.{Environment.NewLine}Error: {string.Join(Environment.NewLine, error)}");
                throw new InvalidOperationException(GetTtnError(error));
            }

            return await CreateApiKey(appid).ConfigureAwait(false);
        }

        public async Task<string> CreateApiKey(string appid)
        {
            if (!_initialized)
            {
                await Init().ConfigureAwait(false);
            }

            Logging.Log.LogInfo($"[{nameof(TtnApiClient)}]({nameof(CreateApiKey)}): Creating API-Key for application: {appid}");

            // Process starten und error/output lesen
            var output = new List<string>();
            var error = new List<string>();
            var resultcode = await RunCliCommand($"application api-keys create {appid} --right-application-all", output, error).ConfigureAwait(false);

            // Fehler bei der Abfrage => Abbruch
            if (resultcode != 0)
            {
                Logging.Log.LogError($"Error while creating api-key for application({appid} on ttn.{Environment.NewLine}Error: {string.Join(Environment.NewLine, error)}");
                throw new InvalidOperationException(GetTtnError(error));
            }

            var response = JObject.Parse(string.Join("", output));
            return (string) (response["key"] ?? "error")!;
        }

        public async Task DeleteDevice(string appid, string deviceid)
        {
            if (!_initialized)
            {
                await Init().ConfigureAwait(false);
            }
            //.\ttn-lw-cli.exe device delete --application-id dataskop --device-id testdevicedataskop

            Logging.Log.LogInfo($"[{nameof(TtnApiClient)}]({nameof(DeleteDevice)}): Deleting Device with AppID: {appid}, DeviceID: {deviceid}");

            // Process starten und error/output lesen
            var error = new List<string>();
            var resultcode = await RunCliCommand($"device delete --application-id {appid} --device-id {deviceid}", error: error).ConfigureAwait(false);

            // Fehler bei der Abfrage => Abbruch
            if (resultcode != 0)
            {
                Logging.Log.LogError($"Error while deleting device on ttn.{Environment.NewLine}Error: {string.Join(Environment.NewLine, error)}");
                throw new InvalidOperationException(GetTtnError(error));
            }
        }

        /// <summary>
        ///     Liest alle ttn devices aus einer Applikation aus.
        /// </summary>
        /// <param name="appId">Applicationid</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Fehler bei der Abfrage aufgetreten siehe Logs</exception>
        public async Task<List<GcTtnIotDevice>> GetDevices(string appId)
        {
            // Konfiguration des cli tools / Login
            if (!_initialized)
            {
                await Init().ConfigureAwait(false);
            }

            Logging.Log.LogInfo($"[{nameof(TtnApiClient)}]({nameof(GetDevices)}): Getting all TTN-Devices");

            // Process starten und error/output lesen
            var output = new List<string>();
            var error = new List<string>();
            var resultcode = await RunCliCommand($"devices list {appId}", output, error).ConfigureAwait(false);

            // Fehler bei der Abfrage => Abbruch
            if (resultcode != 0)
            {
                Logging.Log.LogError($"Error while requesting Devices from ttn.{Environment.NewLine}Error: {string.Join(Environment.NewLine, error)}");
                throw new InvalidOperationException(GetTtnError(error));
            }

            // Result ist eine Liste von Endgeräten. Extrahiere die device_id
            var devices = JArray.Parse(string.Join("", output));
            return devices.Select(t => new GcTtnIotDevice
            {
                DeviceId = (string) t["ids"]?["device_id"]!
            }).ToList();
        }

        /// <summary>
        ///     Uses the Ttnclitool to transfer the OPcode. Can only be used in a windows Environment.
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="appid"></param>
        /// <param name="opcode"></param>
        /// <param name="batchsize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task TransferOpCode(string deviceID, string appid, byte[] opcode, byte batchsize = 20)
        {
            if (opcode == null)
            {
                throw new ArgumentNullException(nameof(opcode));
            }

            if (batchsize <= 1)
            {
                batchsize = 1;
            }

            var startIndex = 0;

            Logging.Log.LogInfo($"[{nameof(TtnApiClient)}]({nameof(TransferOpCode)}): Transfering Bytecode: {Convert.ToHexString(opcode)}");

            while (opcode.Any())
            {
                var lastTransmission = opcode.Length < batchsize;
                var batch = opcode.Take(batchsize).Prepend((byte) startIndex);
                if (lastTransmission)
                {
                    batch = batch.Append((byte) 0x00);
                }

                await AppendDownlink(deviceID, appid, batch.ToArray(), (byte) (lastTransmission ? 11 : 10), true).ConfigureAwait(false);

                opcode = opcode.Skip(batchsize).ToArray();
                startIndex += batchsize;
            }
        }

        public async Task TransferByteArray(string deviceID, string appid, byte[] byteArray, byte port, bool confirmed)
        {
            if (byteArray == null)
            {
                throw new ArgumentNullException("byteArray");
            }

            Logging.Log.LogInfo($"[{"TtnApiClient"}]({"TransferByteArray"}): Transfering Bytecode: {Convert.ToHexString(byteArray)}");
            await AppendDownlink(deviceID, appid, byteArray, port, confirmed).ConfigureAwait(false);
        }

        /// <summary>
        ///     Fügt einen Downlink zur queue hinzu.
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="appid"></param>
        /// <param name="data"></param>
        /// <param name="port"></param>
        /// <param name="confirmed"></param>
        /// <param name="deleteExisting"></param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public async Task AppendDownlink(string deviceid, string appid, byte[] data, byte port, bool confirmed = false, bool deleteExisting = false)
        {
            // Konfiguration des cli tools / Login
            if (!_initialized)
            {
                await Init().ConfigureAwait(false);
            }

            Logging.Log.LogTrace($"[{nameof(TtnApiClient)}]({nameof(AppendDownlink)}): Adding Downlink to queue with Data: {Convert.ToHexString(data)}, AppID: {appid}, DeviceID: {deviceid}, Port: {port}, Confirmed: {confirmed}, Overwrite Existing: {deleteExisting}");

            var replaceString = deleteExisting ? "replace" : "push";
            var confirmedString = confirmed ? " --confirmed" : "";
            var test = BitConverter.ToString(data).Replace("-", "", StringComparison.InvariantCulture);
            var args = $"end-devices downlink {replaceString} {appid} {deviceid} --frm-payload {BitConverter.ToString(data).Replace("-", "", StringComparison.InvariantCulture)}{confirmedString} --f-port {port}";
            var resultCode = await RunCliCommand($"end-devices downlink {replaceString} {appid} {deviceid} --frm-payload {BitConverter.ToString(data).Replace("-", "", StringComparison.InvariantCulture)}{confirmedString} --f-port {port}").ConfigureAwait(false);
        }

        /// <summary>
        ///     Fügt ein Device zu einer ttn-application hinzu
        /// </summary>
        /// <param name="device">Device</param>
        /// <exception cref="ArgumentNullException">device Argument war null</exception>
        /// <exception cref="InvalidOperationException">Fehler bei der Abfrage aufgetreten siehe Logs</exception>
        public async Task<GcTtnIotDevice> AddDevice(GcTtnIotDevice device)
        {
            // Konfiguration des cli tools / Login
            if (!_initialized)
            {
                await Init().ConfigureAwait(false);
            }

            if (device is null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            Logging.Log.LogInfo($"[{nameof(TtnApiClient)}]({nameof(AddDevice)}): Adding new Device to TTN. DeviceID: {device.DeviceId}, AppID: {device.GcTtnCompany.Applicationid}");

            // Clitool process starten
            var error = new List<string>();
            var output = new List<string>();

            // Argumente erzeugen.
            var args = $"devices create --application-id {device.GcTtnCompany.Applicationid}";

            if (!string.IsNullOrWhiteSpace(device.DeviceId))
            {
                args += $" --device-id {device.DeviceId}";
            }

            if (!string.IsNullOrWhiteSpace(device.Description))
            {
                args += $" --description \"{device.Description}\"";
            }

            // Wenn kein app-eui angegeben wurde wird all zeros genutzt.
            if (!string.IsNullOrWhiteSpace(device.AppEui))
            {
                args += $" --join-eui {device.AppEui}";
            }
            else
            {
                args += " --join-eui 0000000000000000";
            }

            // Wenn kein dev-eui angegeben wurde wird die flag --request-dev-eui genutzt um eine dev-eui von ttn zu bekommen.
            if (!string.IsNullOrWhiteSpace(device.DevEui))
            {
                args += $" --dev-eui {device.DevEui}";
            }
            else
            {
                args += " --request-dev-eui";
            }

            // Wenn kein app-key angegeben wurde wird die flag --with-root-keys genutzt um einen app-key von ttn zu bekommen.
            if (!string.IsNullOrWhiteSpace(device.AppKey))
            {
                args += $" --root-keys.app-key.key {device.AppKey}";
            }
            else
            {
                args += " --with-root-keys";
            }

            args += $" --frequency-plan-id {device.LoraFrequency}";
            args += $" --lorawan-version {device.LorawanVersion}";
            args += $" --lorawan-phy-version {device.LoraPhysicalVersion}";


            var resultcode = await RunCliCommand(args, error: error, output: output).ConfigureAwait(false);


            // Fehler bei der Abfrage => Abbruch
            if (resultcode != 0)
            {
                Logging.Log.LogError($"Error while creating Device on ttn.{Environment.NewLine}Error: {string.Join(Environment.NewLine, error)}");
                throw new InvalidOperationException(GetTtnError(error));
            }

            var outstring = string.Join("", output);
            var generated = JsonStringToDevice(outstring);

            return CopyGeneratedProperties(device, generated);
        }

        public GcTtnIotDevice CopyGeneratedProperties(GcTtnIotDevice original, GcTtnIotDevice generated)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            if (generated == null)
            {
                throw new ArgumentNullException(nameof(generated));
            }

            original.DevEui = generated.DevEui;
            original.AppEui = generated.AppEui;
            original.AppKey = generated.AppKey;
            original.SupportsJoin = generated.SupportsJoin;
            original.LorawanVersion = generated.LorawanVersion;
            original.LoraPhysicalVersion = generated.LoraPhysicalVersion;
            original.LoraFrequency = generated.LoraFrequency;

            return original;
        }

        private GcTtnIotDevice JsonStringToDevice(string json)
        {
            var result = new GcTtnIotDevice();
            var device = JObject.Parse(json);

            result.DeviceId = (string) (device["ids"]?["device_id"] ?? "")!;
            result.DevEui = (string) (device["ids"]?["dev_eui"] ?? "")!;
            result.AppEui = (string) (device["ids"]?["join_eui"] ?? "")!;
            result.Description = (string) (device["description"] ?? "")!;
            result.AppKey = (string) (device["root_keys"]?["app_key"]?["key"] ?? "")!;

            if (bool.TryParse((string) (device["supports_join"] ?? "")!, out var supportsJoin))
            {
                result.SupportsJoin = supportsJoin;
            }

            if (Enum.TryParse((string) (device["lorawan_phy_version"] ?? "")!, out EnumLorawanPhysicalVersion loraphyversion))
            {
                result.LoraPhysicalVersion = loraphyversion;
            }

            if (Enum.TryParse((string) (device["lorawan_version"] ?? "")!, out EnumLorawanVersion loraversion))
            {
                result.LorawanVersion = loraversion;
            }

            if (Enum.TryParse((string) (device["frequency_plan_id"] ?? "")!, out EnumLorawanFrequencyPlanId frequencyplan))
            {
                result.LoraFrequency = frequencyplan;
            }

            return result;
        }

        /// <summary>
        ///     Startet das ttnclitool mit den angegebenen Argumenten.
        /// </summary>
        /// <param name="arguments">Argumente für das tool</param>
        /// <param name="output">In diese Liste werden alle Standardausgaben des clitool geschrieben.</param>
        /// <param name="error">In diese Liste werden alle Errorausgaben des clitool geschrieben.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private Task<int> RunCliCommand(string arguments, List<string>? output = null, List<string>? error = null)
        {
            var tcs = new TaskCompletionSource<int>();

            Logging.Log.LogTrace($"[{nameof(TtnApiClient)}]({nameof(RunCliCommand)}): Running CLICommand: \"{_cliPath} {arguments}\"");

            var process = new Process
            {
                StartInfo = {FileName = _cliPath, Arguments = arguments, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false},
                EnableRaisingEvents = true
            };

            process.Exited += (_, _) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };
            process.OutputDataReceived += (_, ea) => output?.Add(ea.Data ?? "");
            process.ErrorDataReceived += (_, ea) => error?.Add(ea.Data ?? "");

            var started = process.Start();
            if (!started)
            {
                //you may allow for the process to be re-used (started = false) 
                //but I'm not sure about the guarantees of the Exited event in such a case
                throw new InvalidOperationException("Could not start process: " + process);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }

        private string GetTtnError(List<string> error)
        {
            var match = Regex.Match(error[0], @"\((.*)\)");
            // ReSharper disable once RedundantAssignment
            var errormsg = string.Empty;
            if (match.Groups.Count >= 0 && !string.IsNullOrEmpty(match.Groups[1].Value))
            {
                errormsg = match.Groups[1].Value;
            }
            else
            {
                errormsg = string.Join(Environment.NewLine, error);
            }

            return errormsg;
        }
    }
}