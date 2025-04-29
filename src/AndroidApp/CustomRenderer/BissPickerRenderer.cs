// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using AndroidApp.CustomRenderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Picker), typeof(BissPickerRenderer))]

namespace AndroidApp.CustomRenderer
{
    /// <summary>
    ///     <para>BissEntryRenderer</para>
    ///     Klasse BissEntryRenderer. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BissPickerRenderer : PickerRenderer
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly Context _context;

        public BissPickerRenderer(Context context) : base(context)
        {
            _context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                var view = Element;


                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Control.BackgroundTintList = ColorStateList.ValueOf(view.TextColor.ToAndroid());
                }
                else
#pragma warning disable CS0618 // Type or member is obsolete
                {
                    Control.Background.SetColorFilter(view.TextColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}