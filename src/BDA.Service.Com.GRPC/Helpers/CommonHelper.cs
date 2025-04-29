// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;
using BDA.Service.Com.GRPC.Protos;

namespace BDA.Service.Com.GRPC.Helpers
{
    /// <summary>
    ///     <para>This class contains helper methods for the gRPC services.</para>
    ///     Klasse CommonHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class CommonHelper
    {
        public static ProtoEnumValueTypes ConvertValueTypeToProtoValueType(EnumValueTypes valueType)
        {
            var protoValueType = default(ProtoEnumValueTypes);
            switch (valueType)
            {
                case EnumValueTypes.Bit:
                    protoValueType = ProtoEnumValueTypes.ValueTypeBit;
                    break;
                case EnumValueTypes.Data:
                    protoValueType = ProtoEnumValueTypes.ValueTypeData;
                    break;
                case EnumValueTypes.Image:
                    protoValueType = ProtoEnumValueTypes.ValueTypeImage;
                    break;
                case EnumValueTypes.Number:
                    protoValueType = ProtoEnumValueTypes.ValueTypeNumber;
                    break;
                case EnumValueTypes.Text:
                    protoValueType = ProtoEnumValueTypes.ValueTypeText;
                    break;
            }

            return protoValueType;
        }

        public static EnumValueTypes ConvertProtoValueTypeToValueType(ProtoEnumValueTypes protoValueType)
        {
            var valueType = default(EnumValueTypes);
            switch (protoValueType)
            {
                case ProtoEnumValueTypes.ValueTypeBit:
                    valueType = EnumValueTypes.Bit;
                    break;
                case ProtoEnumValueTypes.ValueTypeData:
                    valueType = EnumValueTypes.Data;
                    break;
                case ProtoEnumValueTypes.ValueTypeImage:
                    valueType = EnumValueTypes.Image;
                    break;
                case ProtoEnumValueTypes.ValueTypeNumber:
                    valueType = EnumValueTypes.Number;
                    break;
                case ProtoEnumValueTypes.ValueTypeText:
                    valueType = EnumValueTypes.Text;
                    break;
            }

            return valueType;
        }


        public static ProtoResult CreateResult(string message, bool success)
        {
            return new ProtoResult
            {
                Message = message,
                Success = success
            };
        }
    }
}