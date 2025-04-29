// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.GatewayService;
using BDA.Gateway.Com.Base;
using Biss.Log.Producer;
using Biss.Serialize;

namespace BDA.Gateway.Com.Tcp
{
    /// <summary>
    ///     Communication for tcp for a device
    /// </summary>
    public class GatewayComTcp : GatewayComBase
    {
        /// <summary>
        ///     Action what to do when i want to send data in here
        /// </summary>
        public readonly Action<string, byte[]> SendAction;

#pragma warning disable CS0169 // Field is never used
        private DateTime _lastMessage;
#pragma warning restore CS0169 // Field is never used

        private byte[]? _lastOpCode;

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="config"></param>
        /// <param name="iotDevice"></param>
        /// <param name="sendAction"></param>
        public GatewayComTcp(ExGwServiceGatewayConfig config, ExGwServiceIotDeviceConfig iotDevice, Action<string, byte[]> sendAction) : base(config.DbId, iotDevice)
        {
            SendAction = sendAction;
        }

        /// <summary>
        ///     Updating the Device State
        /// </summary>
        /// <param name="state"></param>
        /// <param name="firmwareVersion"></param>
        /// <returns></returns>
        public new async Task UpdateIotDeviceState(EnumDeviceOnlineState state, string firmwareVersion) => await base.UpdateIotDeviceState(state, firmwareVersion).ConfigureAwait(true);

        /// <summary>
        ///     Transfering the configuration
        /// </summary>
        /// <param name="iotDeviceConfig"></param>
        /// <param name="resendConfig">opcode will be sent anyway(without comparing if its new)</param>
        /// <returns></returns>
        protected override async Task<bool> TransferConfig(ExGwServiceIotDeviceConfig iotDeviceConfig, bool resendConfig = false)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (iotDeviceConfig is null)
            {
                return false;
            }

            if (iotDeviceConfig.ConfigVersion == iotDeviceConfig.ConfigVersionService && !resendConfig)
            {
                return true;
            }

            var opcodes = ToStateMachineEmbedded(iotDeviceConfig);

            //Prüfung ob sich OP-Code verandert hat
            if (_lastOpCode != null && _lastOpCode.Length == opcodes.Length && !resendConfig)
            {
                if (_lastOpCode.SequenceEqual(opcodes) /*!changed*/)
                {
                    Logging.Log.LogInfo($"[{nameof(GatewayComTcp)}]({nameof(TransferConfig)}): No changes in Op-Code! No Transfer!");
                    return true;
                }
            }

            SendAction.Invoke(iotDeviceConfig.Secret, opcodes);
            await Task.Delay(500).ConfigureAwait(true);

            _lastOpCode = opcodes;
            return true;
        }

        /// <summary>
        ///     Transfering the configuration
        /// </summary>
        /// <param name="iotDeviceConfig"></param>
        /// <param name="resend">opcode will be sent anyway(without comparing if its new)</param>
        /// <returns></returns>
        [Obsolete]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected async Task<bool> TransferConfigWithoutOpcodes(ExGwServiceIotDeviceConfig iotDeviceConfig, bool resend = false)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (iotDeviceConfig is null)
            {
                return false;
            }

            if (iotDeviceConfig.ConfigVersion == iotDeviceConfig.ConfigVersionService && !resend)
            {
                return true;
            }

            var sendJson = iotDeviceConfig.ToJson();
            var sendData = Encoding.UTF8.GetBytes(sendJson).ToList();
            sendData.Insert(0, 11); // "need config" code

            SendAction.Invoke(iotDeviceConfig.Secret, sendData.ToArray());
            return true;
        }
    }
}