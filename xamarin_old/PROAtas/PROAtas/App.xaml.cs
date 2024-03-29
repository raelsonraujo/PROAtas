﻿using PROAtas.Assets.Styles;
using PROAtas.Core;
using PROAtas.Services;
using PROAtas.ViewModel;
using PROAtas.Views.Pages;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PROAtas
{
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;

        #region Services

        public IAccessibilityService accessibilityService = DependencyService.Get<IAccessibilityService>();
        public IImageService imageService = DependencyService.Get<IImageService>();
        public IToastService toastService = DependencyService.Get<IToastService>();
        public IPrintService printService = DependencyService.Get<IPrintService>();
        public IAdService adService = DependencyService.Get<IAdService>();

        public IPermissionService permissionService = new PermissionService();
        public IDownloadService downloadService = new DownloadService();
        public IDataService dataService = new DataService();
        public ILogService logService = new LogService();

        #endregion

        #region ViewModels

        public HomeViewModel homeViewModel;
        public AboutViewModel aboutViewModel;
        public SettingsViewModel settingsViewModel;
        public MinuteViewModel minuteViewModel;

        #endregion

        public string clipboard = string.Empty;

        public App()
        {
            // Set custom Flags (mostly experimental ones)
            //Device.SetFlags(new[] { 
            //    "",
            //});

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjE5ODY0QDMxMzcyZTM0MmUzMFJ6YjhtVjhjYnRRdlZOSC9yK0lGcmFTd09PT1dnVHhWcGxhUGNxeXk3ajA9");

            InitializeComponent();

            if (!Current.Properties.ContainsKey(Constants.UserName)) Current.Properties[Constants.UserName] = "Novo Usuário";
            if (!Current.Properties.ContainsKey(Constants.OrganizationName)) Current.Properties[Constants.OrganizationName] = "Nova Organização";
            if (!Current.Properties.ContainsKey(Constants.FontSize)) Current.Properties[Constants.FontSize] = "14";
            if (!Current.Properties.ContainsKey(Constants.FontFamily)) Current.Properties[Constants.FontFamily] = "Times New Roman";
            if (!Current.Properties.ContainsKey(Constants.MarginLeft)) Current.Properties[Constants.MarginLeft] = "3";
            if (!Current.Properties.ContainsKey(Constants.MarginTop)) Current.Properties[Constants.MarginTop] = "2";
            if (!Current.Properties.ContainsKey(Constants.MarginRight)) Current.Properties[Constants.MarginRight] = "3";
            if (!Current.Properties.ContainsKey(Constants.MarginBottom)) Current.Properties[Constants.MarginBottom] = "2";
            if (!Current.Properties.ContainsKey(Constants.Version)) Current.Properties[Constants.Version] = "0";
            if (!Current.Properties.ContainsKey(Constants.SelectedMinuteImage)) Current.Properties[Constants.SelectedMinuteImage] = "0";
            Current.SavePropertiesAsync();

            // ViewModels
            homeViewModel = new HomeViewModel();
            aboutViewModel = new AboutViewModel();
            settingsViewModel = new SettingsViewModel();
            minuteViewModel = new MinuteViewModel();

            MainPage = new AppShell { } .Standard();

            var version = App.Current.Properties[Constants.Version]?.ToString();
            if (version != "v13")
            {
                Current.Properties[Constants.Version] = "v13";
                Current.SavePropertiesAsync();

                Application.Current.MainPage.DisplayAlert("Versão 13", "Olá!\r\n\r\nEsta é a versão 13 do PRO Atas. As principais mudanças são:\r\n\r\n- Agora é possível mudar a imagem do cabeçalho da ata em 'Configurações';\r\n- Página 'Sobre Mim' acrescentada;\r\n- Performance do aplicativo substancialmente melhorada;\r\n- Melhorias gerais na experiência de uso;\r\n- Alguns bugs foram corrigidos;\r\n\r\nEste aplicativo é desenvolvido por Raelson por iniciativa própria. Caso precise entrar em contato, enviar e-mail para raelsoncraftz@gmail.com", "Legal!");
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
