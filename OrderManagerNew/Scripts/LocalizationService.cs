using OrderManagerNew.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
//ref:https://www.codeproject.com/Articles/299436/WPF-Localization-for-Dummies
//ref:https://stackoverflow.com/questions/50292087/dynamic-localized-wpf-application-with-resource-files
//ref:https://codinginfinity.me/post/2015-05-10/localization_of_a_wpf_app_the_simple_approach
//總之就是自己寫一個binding

/*
*實作方法: 
* A).取字串 
* TranslationSource.Instance[{"Key"}]
* Example:
*C#
**string str = EzOrthoUI.TranslationSource.Instance["ModelPosition_Btn_StartDefine"];
*
*C++
**String str = EzOrthoUI::TranslationSource::Instance["ModelPosition_Btn_StartDefine"];
* 
* B).切換語系
* LocalizationService.SetLanguage({國家代稱});
* Example
* C#
* LocalizationService.SetLanguage("zh-TW");
* 
* WPF_xaml
* xmlns:local="clr-namespace:WPFUI"
* ToolTip="{local:LocExtension TestString}"
* 
* C++
* LocalizationService::SetLanguage("zh-TW");
*/
namespace OrderManagerNew
{
    public class LocalizationService
    {
        public static void SetLanguage(string locale)
        {
            if (string.IsNullOrEmpty(locale))
                locale = "en-US";
            TranslationSource.Instance.CurrentCulture = new System.Globalization.CultureInfo(locale);
        }
    }

    public class TranslationSource : INotifyPropertyChanged
    {
        private static readonly TranslationSource instance = new TranslationSource();

        public static TranslationSource Instance
        {
            get { return instance; }
        }

        private readonly ResourceManager resManager = StringResources.ResourceManager;
        private CultureInfo currentCulture = null;

        public string this[string key]
        {
            get { return this.resManager.GetString(key, this.currentCulture); }
        }

        public CultureInfo CurrentCulture
        {
            get { return this.currentCulture; }
            set
            {
                if (this.currentCulture != value)
                {
                    this.currentCulture = value;
                    var @event = this.PropertyChanged;
                    if (@event != null)
                    {
                        @event.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class LocExtension : Binding
    {
        public LocExtension(string name)
            : base("[" + name + "]")
        {
            this.Source = TranslationSource.Instance;
        }
    }
}
