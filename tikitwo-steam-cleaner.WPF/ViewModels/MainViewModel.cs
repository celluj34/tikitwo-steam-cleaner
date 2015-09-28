using Prism.Commands;
using Prism.Mvvm;
using tikitwo_steam_cleaner.Application.Services;

namespace tikitwo_steam_cleaner.WPF.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly ISteamFolderService _steamFolderService;
        private string _steamFolder;

        public MainViewModel() : this(Di.Resolve<ISteamFolderService>()) {}

        private MainViewModel(ISteamFolderService steamFolderService)
        {
            _steamFolderService = steamFolderService;
            FindSteamFolder = new DelegateCommand(FindSteamFolderExecute);
            GetSteamFolder = new DelegateCommand(GetSteamFolderExecute);
        }

        public string SteamFolder
        {
            get {return _steamFolder;}
            set
            {
                if(SetProperty(ref _steamFolder, value))
                {
                    //Search.RaiseCanExecuteChanged();
                }
            }
        }

        #region Commands

        #region Find Steam Folder
        public DelegateCommand FindSteamFolder {get;private set;}

        private void FindSteamFolderExecute()
        {
            SteamFolder = _steamFolderService.TryGetSteamFolder();
        }
        #endregion

        #region Get Steam Folder
        public DelegateCommand GetSteamFolder {get;private set;}

        private void GetSteamFolderExecute()
        {
            SteamFolder = _steamFolderService.GetSteamFolder();
        }
        #endregion

        #region Search

        //private bool CanSearch()
        //{
        //    return !string.IsNullOrWhiteSpace(SteamFolder) && Directory.Exists(SteamFolder);
        //}
        #endregion

        #endregion
    }
}