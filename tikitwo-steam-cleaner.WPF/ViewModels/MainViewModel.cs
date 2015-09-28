using System.Collections.ObjectModel;
using System.Linq;
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
            AddFolder = new DelegateCommand(AddFolderExecute, CanAddFolder);
            RemoveCustomFolder = new DelegateCommand(RemoveCustomFolderExecute, CanRemoveCustomFolder);
            Search = new DelegateCommand(SearchExecute, CanSearch);

            CanUseControls = true;
            FoldersToSearch = new ObservableCollection<string>();
        }

        #region Private Backing Fields
        private bool _canUseControls;
        private string _selectedFolder;
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

        public ObservableCollection<string> FoldersToSearch {get;set;}

        public string SelectedFolder
        {
            get {return _selectedFolder;}
            set
            {
                if(SetProperty(ref _selectedFolder, value))
                {
                    UpdateCommands();
                }
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateCommands()
        {
            FindSteamFolder.RaiseCanExecuteChanged();
            AddFolder.RaiseCanExecuteChanged();
            RemoveCustomFolder.RaiseCanExecuteChanged();
            Search.RaiseCanExecuteChanged();
        }

        private void AddFolderToDisplay(string newFolder)
        {
            if(newFolder == null || FoldersToSearch.Contains(newFolder))
            {
                return;
            }

            FoldersToSearch.Add(newFolder);

            UpdateCommands();
        }
        #endregion

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

        #region Add Folder
        public DelegateCommand AddFolder {get;}

        private void AddFolderExecute()
        {
            var newFolder = _steamFolderService.GetFolder();

            AddFolderToDisplay(newFolder);
        }

        private bool CanAddFolder()
        {
            return CanUseControls;
        }
        #endregion

        #region Remove Custom Folder
        public DelegateCommand RemoveCustomFolder {get;}

        private void RemoveCustomFolderExecute()
        {
            FoldersToSearch.Remove(SelectedFolder);
        }

        private bool CanRemoveCustomFolder()
        {
            return CanUseControls && FoldersToSearch.Any() && SelectedFolder != null;
        }
        #endregion

        #region Search
        public DelegateCommand Search {get;}

        private void SearchExecute()
        {
            CanUseControls = false;
        }

        private bool CanSearch()
        {
            return CanUseControls && FoldersToSearch.Any();
        }
        #endregion

        #endregion
    }
}