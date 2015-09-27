using Prism.Mvvm;

namespace tikitwo_steam_cleaner.UAP.ViewModels
{
    public class MainViewModel : BindableBase
    {

        private string _steamFolder;
        public string SteamFolder
        {
            get {return _steamFolder;}
            set
            {
                if(SetProperty(ref _steamFolder, value))
                {
                    //alert or something
                }
            }
        }
    }
}