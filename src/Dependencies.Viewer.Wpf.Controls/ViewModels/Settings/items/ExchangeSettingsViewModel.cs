﻿using System.Collections.Generic;
using System.Linq;
using Dependencies.Exchange.Base;

namespace Dependencies.Viewer.Wpf.Controls.ViewModels.Settings
{
    public class ExchangeSettingsViewModel
    {

        public ExchangeSettingsViewModel(IEnumerable<ISettingUpdaterProvider> settingUpdaterProviders) => SettingUpdaterProviders = settingUpdaterProviders.ToList();

        public IList<ISettingUpdaterProvider> SettingUpdaterProviders { get; }
    }
}