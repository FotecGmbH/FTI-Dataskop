// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using AndroidApp.CustomRenderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Entry), typeof(BissEntryRenderer))]

namespace AndroidApp.CustomRenderer
{
    /// <summary>
    ///     <para>BissEntryRenderer</para>
    ///     Klasse BissEntryRenderer. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BissEntryRenderer : EntryRenderer
    {
        private readonly Context _context;

        public BissEntryRenderer(Context context) : base(context)
        {
            _context = context;
        }

        public static float DpToPixels(Context context, float valueInDp)
        {
            var metrics = context.Resources.DisplayMetrics;
            return TypedValue.ApplyDimension(ComplexUnitType.Dip, valueInDp, metrics);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            var view = Element;


            var gradientBackground = new GradientDrawable();
            //gradientBackground.SetStroke(1, view.BackgroundColor.ToAndroid());
            gradientBackground.SetStroke(1, view.TextColor.ToAndroid());
            gradientBackground.SetCornerRadius(DpToPixels(_context, Convert.ToSingle(8)));
            gradientBackground.SetShape(ShapeType.Rectangle);
            gradientBackground.SetColor(view.BackgroundColor.ToAndroid());
            Control.SetBackground(gradientBackground);
            SetBackgroundColor(Color.Transparent.ToAndroid());

            // Set padding for the internal text from border  
            Control.SetPadding(
                (int) DpToPixels(_context, Convert.ToSingle(12)), Control.PaddingTop,
                (int) DpToPixels(_context, Convert.ToSingle(12)), Control.PaddingBottom);
        }
    }
}