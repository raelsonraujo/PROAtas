﻿using PROAtas.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace PROAtas.Views.Pages
{
    public class AboutPage : BasePage<AboutViewModel>
    {
        public AboutPage() => Build();

        private void Build()
        {
            var app = App.Current;
            var vm = ViewModel = App.Current.aboutViewModel;

        }
    }
}
