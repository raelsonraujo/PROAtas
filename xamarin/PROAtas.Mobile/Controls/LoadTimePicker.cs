﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace PROAtas.Mobile.Controls
{
    public class LoadTimePicker : TimePicker
    {
        public LoadTimePicker()
        {
            PropertyChanged += LoadTimePicker_TimeChanged;
        }

        ~LoadTimePicker()
        {
            PropertyChanged -= LoadTimePicker_TimeChanged;
        }

        public ICommand LoadCommand
        {
            get { return (Command)GetValue(LoadCommandProperty); }
            set { SetValue(LoadCommandProperty, value); }
        }
        public static readonly BindableProperty LoadCommandProperty = BindableProperty.Create(nameof(LoadCommand), typeof(ICommand), typeof(LoadTimePicker), default(ICommand));

        private void LoadTimePicker_TimeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsFocused && e.PropertyName == TimeProperty.PropertyName && Time != TimeSpan.MinValue)
                LoadCommand?.Execute(Time);
        }
    }
}
