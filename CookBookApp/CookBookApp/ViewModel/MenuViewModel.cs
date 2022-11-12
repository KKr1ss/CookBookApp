﻿using CookBookApp.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace CookBookApp.ViewModel
{
    public class MenuViewModel : BaseViewModel
    {
        public MenuViewModel()
        {
            InitializeSettings();
        }

        public void InitializeSettings()
        {
            setLanguage();
        }
        public void setLanguage()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("hu-HU");
        }

    }
}