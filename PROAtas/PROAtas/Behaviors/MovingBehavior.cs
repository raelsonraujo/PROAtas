﻿using PROAtas.Core;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PROAtas.Behaviors
{
    public class MovingBehavior : Behavior<VisualElement>
    {
        public enum EMoveTo { Start, Top, End, Bottom, }

        private VisualElement Control;

        #region Behavior Implementation

        private void BindingContextAttached(object sender, EventArgs args)
        {
            this.BindingContext = ((BindableObject)sender).BindingContext;
        }

        protected override void OnAttachedTo(VisualElement control)
        {
            Control = control;
            control.BindingContextChanged += BindingContextAttached;

            base.OnAttachedTo(control);
        }

        protected override void OnDetachingFrom(VisualElement control)
        {
            Control = null;
            control.BindingContextChanged -= BindingContextAttached;

            base.OnDetachingFrom(control);
        }

        #endregion


        #region Bindable Properties

        public EMoveTo MoveTo { get; set; }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
        public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(MovingBehavior), default(bool), propertyChanged: OpenAnimation);

        #endregion

        #region Property Changed Events

        private static async void OpenAnimation(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MovingBehavior)bindable;
            await control.OpenAnimationExecution();
        }
        protected async Task OpenAnimationExecution()
        {
            if (Control != null)
            {
                await Task.Delay(100);
                if (IsActive) await Control.TranslateTo(0, 0, 500, Easing.CubicOut);
                else
                    switch (MoveTo)
                    {
                        case EMoveTo.Start:
                            await Control.TranslateTo(-Control.Width, 0, 500, Easing.CubicOut);
                            break;
                        case EMoveTo.Top:
                            await Control.TranslateTo(0, -Control.Height, 500, Easing.CubicOut);
                            break;
                        case EMoveTo.End:
                            await Control.TranslateTo(Control.Width, 0, 500, Easing.CubicOut);
                            break;
                        case EMoveTo.Bottom:
                            await Control.TranslateTo(0, Control.Height, 500, Easing.CubicOut);
                            break;
                    }
            }
        }

        #endregion
    }
}
