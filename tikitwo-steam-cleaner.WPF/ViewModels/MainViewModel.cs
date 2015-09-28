using Prism.Commands;
using Prism.Mvvm;
using tikitwo_steam_cleaner.Application.Services;

namespace tikitwo_steam_cleaner.WPF.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly ISteamFolderService _steamFolderService;

        public MainViewModel() : this(Di.Resolve<ISteamFolderService>()) {}

        private MainViewModel(ISteamFolderService steamFolderService)
        {
            _steamFolderService = steamFolderService;

            FindSteamFolder = new DelegateCommand(FindSteamFolderExecute, CanFindSteamFolder);

            CanUseControls = true;
        }

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


        {
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