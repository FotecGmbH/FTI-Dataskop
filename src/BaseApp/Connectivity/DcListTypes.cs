// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Configs.NewValueNotifications;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Dc.Client;
using Biss.Dc.Core;

namespace BaseApp.Connectivity
{
    /// <summary>
    ///     Hilfsklasse für X:Datatype in xaml
    /// </summary>
    public class DcListTypeGateway : DcListDataPoint<ExGateway>
    {
        /// <summary>
        ///     Listentyp für Orgusers konstruktor
        /// </summary>
        /// <param name="root"></param>
        /// <param name="data"></param>
        /// <param name="pointId"></param>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <param name="state"></param>
        /// <param name="sortIdex"></param>
        /// <param name="dataVersion"></param>
        public DcListTypeGateway(IDcDataRoot root, ExGateway data, string pointId, EnumDcDataSource source, long index, EnumDcListElementState state, long sortIdex, byte[] dataVersion) :
            base(root, data, pointId, source, index, state, sortIdex, dataVersion)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        public DcListTypeGateway(ExGateway data) : base(data)
        {
        }
    }


    /// <summary>
    ///     Hilfsklasse für X:Datatype in xaml
    /// </summary>
    public class DcListTypeIotDevice : DcListDataPoint<ExIotDevice>
    {
        /// <summary>
        ///     Listentyp für Orgusers konstruktor
        /// </summary>
        /// <param name="root"></param>
        /// <param name="data"></param>
        /// <param name="pointId"></param>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <param name="state"></param>
        /// <param name="sortIdex"></param>
        /// <param name="dataVersion"></param>
        public DcListTypeIotDevice(IDcDataRoot root, ExIotDevice data, string pointId, EnumDcDataSource source, long index, EnumDcListElementState state, long sortIdex, byte[] dataVersion) :
            base(root, data, pointId, source, index, state, sortIdex, dataVersion)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        public DcListTypeIotDevice(ExIotDevice data) : base(data)
        {
        }
    }

    /// <summary>
    ///     Hilfsklasse für X:Datatype in xaml
    /// </summary>
    public class DcListTypeMeasurementDefinition : DcListDataPoint<ExMeasurementDefinition>
    {
        /// <summary>
        ///     Listentyp für Orgusers konstruktor
        /// </summary>
        /// <param name="root"></param>
        /// <param name="data"></param>
        /// <param name="pointId"></param>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <param name="state"></param>
        /// <param name="sortIdex"></param>
        /// <param name="dataVersion"></param>
        public DcListTypeMeasurementDefinition(IDcDataRoot root, ExMeasurementDefinition data, string pointId, EnumDcDataSource source, long index, EnumDcListElementState state, long sortIdex, byte[] dataVersion) :
            base(root, data, pointId, source, index, state, sortIdex, dataVersion)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        public DcListTypeMeasurementDefinition(ExMeasurementDefinition data) : base(data)
        {
        }
    }

    /// <summary>
    ///     Hilfsklasse für X:Datatype in xaml
    /// </summary>
    public class DcListTypeCompany : DcListDataPoint<ExCompany>
    {
        /// <summary>
        ///     Listentyp für Orgusers konstruktor
        /// </summary>
        /// <param name="root"></param>
        /// <param name="data"></param>
        /// <param name="pointId"></param>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <param name="state"></param>
        /// <param name="sortIdex"></param>
        /// <param name="dataVersion"></param>
        public DcListTypeCompany(IDcDataRoot root, ExCompany data, string pointId, EnumDcDataSource source, long index, EnumDcListElementState state, long sortIdex, byte[] dataVersion) :
            base(root, data, pointId, source, index, state, sortIdex, dataVersion)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        public DcListTypeCompany(ExCompany data) : base(data)
        {
        }
    }

    /// <summary>
    ///     List Type Project.
    /// </summary>
    public class DcListTypeProject : DcListDataPoint<ExProject>
    {
        /// <summary>
        ///     Listentyp für Orgusers konstruktor
        /// </summary>
        /// <param name="root"></param>
        /// <param name="data"></param>
        /// <param name="pointId"></param>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <param name="state"></param>
        /// <param name="sortIdex"></param>
        /// <param name="dataVersion"></param>
        public DcListTypeProject(IDcDataRoot root, ExProject data, string pointId, EnumDcDataSource source, long index, EnumDcListElementState state, long sortIdex, byte[] dataVersion) :
            base(root, data, pointId, source, index, state, sortIdex, dataVersion)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        public DcListTypeProject(ExProject data) : base(data)
        {
        }
    }

    /// <summary>
    ///     List Type Project.
    /// </summary>
    public class DcListTypeNewValueNotification : DcListDataPoint<ExNewValueNotification>
    {
        /// <summary>
        ///     Listentyp für Orgusers konstruktor
        /// </summary>
        /// <param name="root"></param>
        /// <param name="data"></param>
        /// <param name="pointId"></param>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <param name="state"></param>
        /// <param name="sortIdex"></param>
        /// <param name="dataVersion"></param>
        public DcListTypeNewValueNotification(IDcDataRoot root, ExNewValueNotification data, string pointId, EnumDcDataSource source, long index, EnumDcListElementState state, long sortIdex, byte[] dataVersion) :
            base(root, data, pointId, source, index, state, sortIdex, dataVersion)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        public DcListTypeNewValueNotification(ExNewValueNotification data) : base(data)
        {
        }
    }
}