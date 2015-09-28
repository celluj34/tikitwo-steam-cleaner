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
            GetSteamFolder = new DelegateCommand(GetSteamFolderExecute);

            FindSteamFolder = new DelegateCommand(FindSteamFolderExecute, CanFindSteamFolder);

            CanUseControls = true;
        }

        public string SteamFolder
        #region Private Backing Fields
        private bool _canUseControls;
        #endregion

        #region Public Properties
        public bool CanUseControls
        {
            get {return _canUseControls;}
            set
            {
                if(SetProperty(ref _canUseControls, value))
                {
                    UpdateCommands();
                }
            }
        }


        #region Commands

        #region Find Steam Folder
        public DelegateCommand FindSteamFolder {get;}

        private void FindSteamFolderExecute()
        {
            var newFolder = _steamFolderService.TryGetSteamFolder();

            AddFolderToDisplay(newFolder);
        }

        private bool CanFindSteamFolder()
        {
            return CanUseControls;
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