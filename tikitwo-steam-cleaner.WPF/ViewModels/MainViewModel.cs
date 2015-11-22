using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using tikitwo_steam_cleaner.Application.Models;
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
            RemoveFolder = new DelegateCommand(RemoveFolderExecute, CanRemoveFolder);
            Search = new DelegateCommand(SearchExecute, CanSearch);
            DeletePackages = new DelegateCommand(DeletePackagesExecute, CanDeletePackages);

            CanUseControls = true;
            FoldersToSearch = new ObservableCollection<string>();
            FoldersToDelete = new ObservableCollection<RedistItem>();
        }

        #region Private Backing Fields
        private bool _canUseControls;
        private string _selectedFolder;
        private int _packagesFound;
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

        public int PackagesFound
        {
            get {return _packagesFound;}
            set {SetProperty(ref _packagesFound, value);}
        }

        public ObservableCollection<string> FoldersToSearch {get;set;}
        public ObservableCollection<RedistItem> FoldersToDelete {get;set;}
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

        private async Task RunAsyncMethod(Action task)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => CanUseControls = false);

            await Task.Run(task);

            System.Windows.Application.Current.Dispatcher.Invoke(() => CanUseControls = true);
        }
        #endregion

        #region Commands

        #region Find Steam Folder
        public DelegateCommand FindSteamFolder {get;}

        private void FindSteamFolderExecute()
        {
            var possibleSteamFolders = _steamFolderService.TryGetSteamFolder();

            foreach(var steamFolder in possibleSteamFolders)
            {
                AddFolderToDisplay(steamFolder);
            }
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
            await RunAsyncMethod(() =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    FoldersToDelete.Clear();
                    PackagesFound = 0;
                });

                var foldersToSearch = FoldersToSearch.ToList();

                var folders = _steamFolderService.Search(foldersToSearch).OrderBy(x => x.Path);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    FoldersToDelete.AddRange(folders);
                    PackagesFound = folders.Count();
                });
            });
        }

        private bool CanSearch()
        {
            return CanUseControls && FoldersToSearch.Any();
        }
        #endregion

        #region Remove Packages
        public DelegateCommand DeletePackages {get;}

        private async void DeletePackagesExecute()
        {
            await RunAsyncMethod(() =>
            {
                var itemsToDelete = FoldersToDelete.Where(x => x.Selected).ToList();

                var deletedFolders = _steamFolderService.Delete(itemsToDelete);

                long totalSaved = 0;
                deletedFolders.ForEach(x =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => FoldersToDelete.Remove(x));

                    totalSaved += x.SizeInBytes;
                });

                MessageBox.Show("You saved " + totalSaved + " bytes!");
            });
        }

        private bool CanDeletePackages()
        {
            return CanUseControls && FoldersToDelete.Any();
        }
        #endregion

        #endregion
    }
}