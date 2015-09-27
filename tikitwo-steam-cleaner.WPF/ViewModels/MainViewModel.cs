using Prism.Mvvm;

namespace tikitwo_steam_cleaner.WPF.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private string _steamFolder;

        //public MainViewModel() : this(Di.Resolve<IDateTimeService>()) {}

        //public MainViewModel(IDateTimeService dateTimeService)
        //{
        //    _dateTimeService = dateTimeService;
        //}

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