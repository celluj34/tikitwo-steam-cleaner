using System.Windows;

namespace tikitwo_steam_cleaner.WPF
{
    public partial class App
    {
        #region Overrides of Application
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Di.ConfigureContainer();
        }
        #endregion
    }
}