using System.Windows;

namespace tikitwo_steam_cleaner.WPF
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Di.ConfigureContainer();
        }
    }
}