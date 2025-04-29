// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using Biss.Apps.Attributes;
using Biss.Apps.Push;
using Biss.Apps.ViewModel;
using Biss.Dc.Core;
using Biss.Interfaces;
using Exchange.Resources;
using Newtonsoft.Json;


namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Push Settings der App</para>
    ///     Klasse VMPush. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewSettingPush", true)]
    public class VmSettingsPush : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceExUserDevice}"
        /// </summary>
        public static EnumSelectable<EnumPushTopics> DesignInstanceEnumPushTopic = new EnumSelectable<EnumPushTopics>(EnumPushTopics.TestPushA);

        private bool _setting10MinutePushEnabled;

        /// <summary>
        ///     VMPush
        /// </summary>
        public VmSettingsPush() : base(ResViewSettingsPush.LblTitle, subTitle: ResViewSettingsPush.LblSubTitle)
        {
            SetViewProperties(true);


            MenuGestureEnabled = false;

            _setting10MinutePushEnabled = Dc.DcExUser.Data.Setting10MinPush;
        }

        #region Properties

        /// <summary>
        ///     Design Instanz.
        /// </summary>
        public static VmSettingsPush DesignInstance => new VmSettingsPush();

        /// <summary>
        ///     Ist Push enabled.
        /// </summary>
        public bool PushEnabled { get; set; }

        /// <summary>
        ///     10 Minuten Push
        /// </summary>
        public bool Setting10MinutePushEnabled
        {
            get => _setting10MinutePushEnabled;
            set => Update10MinutePush(value);
        }

        /// <summary>
        ///     Settings öffnen
        /// </summary>
        public VmCommand CmdOpenSettings { get; private set; } = null!;

        /// <summary>
        ///     Anforderung um eine Push über den Server zu schicken.
        /// </summary>
        public VmCommand CmdSendPush { get; private set; } = null!;

        /// <summary>
        ///     Abonnierte Topics.
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public ObservableCollection<EnumSelectable<EnumPushTopics>> Topics { get; set; } = new ObservableCollection<EnumSelectable<EnumPushTopics>>();
#pragma warning restore CA2227 // Collection properties should be read only

        #endregion

        /// <summary>
        ///     Wird aufgerufen sobald die View initialisiert wurde
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task OnActivated(object? args = null)
        {
            foreach (EnumPushTopics topic in Enum.GetValues(typeof(EnumPushTopics)))
            {
                Topics.Add(new EnumSelectable<EnumPushTopics>(topic));
            }


            if (!string.IsNullOrWhiteSpace(Dc.DcExUser.Data.PushTags))
            {
                var dcTopics = JsonConvert.DeserializeObject<List<string>>(Dc.DcExUser.Data.PushTags);
                if (dcTopics != null)
                {
                    UpdateSelectedTopics(dcTopics, true);
                }
            }

            Push.PushStateChanged += Push_PushStateChanged;
            PushEnabled = await Push.CheckPushEnabled();
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdOpenSettings = new VmCommand(ResViewSettingsPush.CmdOpenSettings, () => { Push.OpenSettings(); }, glyph: Glyphs.Cog);

            CmdSendPush = new VmCommand(ResViewSettingsPush.CmdSendPush, () =>
            {
                Dc.SendCommonData(new DcCommonData
                {
                    Key = EnumDcCommonCommands.SendTestPush.ToString(),
                    Value = Push.Token
                });
            });
        }

        private async void Topic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var topics = Topics.Where(t => t.Selected).Select(t => t.Enum.ToString());
            var clone = JsonConvert.SerializeObject(Push.SubscribedTopics);
            UpdateSelectedTopics(topics);
            Dc.DcExUser.Data.PushTags = JsonConvert.SerializeObject(Push.SubscribedTopics);

            var r = await Dc.DcExUser.StoreData().ConfigureAwait(true);
            if (!r.DataOk)
            {
                await MsgBox.Show(ResViewSettingsPush.MsgTopicNotSaved, ResViewSettingsPush.MsgTitleTopicNotSaved).ConfigureAwait(true);
#pragma warning disable CS8604 // Possible null reference argument.
                UpdateSelectedTopics(JsonConvert.DeserializeObject<List<string>>(clone), true);
#pragma warning restore CS8604 // Possible null reference argument.
            }
        }

        private void Push_PushStateChanged(object sender, PushStateChangedEventArgs e)
        {
            PushEnabled = e.PushEnabled;
        }

        private void UpdateSelectedTopics(IEnumerable<string> topics, bool updateView = false)
        {
            if (topics != null!)
            {
                Push.UpdateTopics(topics.ToList());
            }
            else
            {
                foreach (var topic in Topics.Select(t => t.Enum.ToString()))
                {
                    Push.UnsubscribeFromTopic(topic);
                }
            }

            if (!updateView)
            {
                return;
            }

            foreach (var topic in Topics)
            {
                topic.PropertyChanged -= Topic_PropertyChanged;
                if (Push.SubscribedTopics.Any(t => t.Equals(topic.Enum.ToString().ToUpperInvariant(), StringComparison.InvariantCulture)))
                {
                    topic.Selected = true;
                }

                topic.PropertyChanged += Topic_PropertyChanged;
            }
        }

        private async void Update10MinutePush(bool value)
        {
            Dc.DcExUser.Data.Setting10MinPush = value;
            var store = await Dc.DcExUser.StoreData().ConfigureAwait(true);

            if (!store.DataOk)
            {
                await MsgBox.Show(ResViewSettingsPush.MsgSettingsNotSaved, ResViewSettingsPush.MsgTitleSettingsNotSaved).ConfigureAwait(true);
                Dc.DcExUser.Data.Setting10MinPush = !value;
            }
            else
            {
                _setting10MinutePushEnabled = value;
            }
        }
    }

    /// <summary>
    ///     Selectable Enum
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumSelectable<T> : IBissModel where T : Enum
    {
        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="enumToExtend"></param>
        public EnumSelectable(T enumToExtend)
        {
            Enum = enumToExtend;
        }

        #region Properties

        /// <summary>
        ///     Der Enum
        /// </summary>
        public T Enum { get; }


        /// <summary>
        ///     Ist das Enum selektiert
        /// </summary>
        public bool Selected { get; set; }

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS0414
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0414
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #endregion
    }
}