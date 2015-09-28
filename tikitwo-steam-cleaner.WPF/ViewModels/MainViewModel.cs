using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using tikitwo_steam_cleaner.Application.Services;
using tikitwo_steam_cleaner.WPF.Models;

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
            RemoveFolder = new DelegateCommand(RemoveFolderExecute, CanRemoveFolder);
            Search = new DelegateCommand(SearchExecute, CanSearch);
            DeletePackages = new DelegateCommand(DeletePackagesExecute, CanDeletePackages);

            CanUseControls = true;
            FoldersToSearch = new ObservableCollection<string>();
            FoldersToDelete = new ObservableCollection<FolderThing>();
        }

        #region Private Backing Fields
        private bool _canUseControls;
        private string _selectedFolder;
        #endregion

        #region Public Properties
        private bool CanUseControls
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

        public ObservableCollection<string> FoldersToSearch {get;set;}
        public ObservableCollection<FolderThing> FoldersToDelete {get;set;}
        #endregion

        #region Helper Methods
        private void UpdateCommands()
        {
            FindSteamFolder.RaiseCanExecuteChanged();
            AddFolder.RaiseCanExecuteChanged();
            RemoveFolder.RaiseCanExecuteChanged();
            Search.RaiseCanExecuteChanged();
            DeletePackages.RaiseCanExecuteChanged();
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

        #region Remove Folder
        public DelegateCommand RemoveFolder {get;}

        private void RemoveFolderExecute()
        {
            FoldersToSearch.Remove(SelectedFolder);
        }

        private bool CanRemoveFolder()
        {
            return CanUseControls && FoldersToSearch.Any() && SelectedFolder != null;
        }
        #endregion

        #region Search
        public DelegateCommand Search {get;}

        private async void SearchExecute()
        {
            await Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => CanUseControls = false);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    FoldersToDelete.Add(new FolderThing {Path = "asdaSD", Selected = false, Size = "asdasda", Type = "12e12edad"});
                    FoldersToDelete.Add(new FolderThing {Path = "576u6y", Selected = false, Size = "asdasda", Type = "p90p08p7"});
                    FoldersToDelete.Add(new FolderThing {Path = "908o7", Selected = true, Size = "12d1d2", Type = "hhuktykj"});
                });

                Thread.Sleep(3000);

                System.Windows.Application.Current.Dispatcher.Invoke(() => CanUseControls = true);
            });
        }

        private bool CanSearch()
        {
            return CanUseControls && FoldersToSearch.Any();
        }
        #endregion

        #region Remove Packages
        public DelegateCommand DeletePackages {get;}

        private void DeletePackagesExecute()
        {
            CanUseControls = false;
        }

        private bool CanDeletePackages()
        {
            return CanUseControls && FoldersToDelete.Any();
        }
        #endregion

        #endregion
    }
}