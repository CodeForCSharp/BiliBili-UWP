using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace bilibili2.Class
{
    class SettingHelper
    {
        private readonly ApplicationDataContainer _container = ApplicationData.Current.LocalSettings;
        private readonly PackageId _pack = (Package.Current).Id;
        public object GetSettingValue(string settingName)
        {
            return _container.Values[settingName];
        }
        public void SetSettingValue(string settingName,object value)
        {
            _container.Values[settingName] = value;
        }
        public bool SettingContains(string settingName)
        {
            return _container.Values[settingName]!=null;
        }
        public string GetVersion()
        {
            return $"{_pack.Version.Major}.{_pack.Version.Minor}.{_pack.Version.Build}.{_pack.Version.Revision}";
        }

    }
}
