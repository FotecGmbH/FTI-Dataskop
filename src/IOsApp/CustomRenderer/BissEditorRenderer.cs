// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using IOsApp.CustomRenderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


//https://www.c-sharpcorner.com/article/xamarin-forms-custom-entry/
[assembly: ExportRenderer(typeof(Editor), typeof(BissEditorRenderer))]

namespace IOsApp.CustomRenderer
{
    /// <summary>
    ///     <para>BissEditorRenderer</para>
    ///     Klasse BissEditorRenderer. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BissEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                var view = Element;

                //Control.LeftView = new UIView(new CGRect(0f, 0f, 9f, 20f));  
                //Control.LeftViewMode = UITextFieldViewMode.Always;  

                Control.KeyboardAppearance = UIKeyboardAppearance.Dark;
                Control.ReturnKeyType = UIReturnKeyType.Done;
                // Radius for the curves  
                Control.Layer.CornerRadius = Convert.ToSingle(8);
                // Thickness of the Border Color  
                Control.Layer.BorderColor = view.BackgroundColor.ToCGColor();
                // Thickness of the Border Width  
                Control.Layer.BorderWidth = 1;
                Control.ClipsToBounds = true;
            }
        }
    }
}