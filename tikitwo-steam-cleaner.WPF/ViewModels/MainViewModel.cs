using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Prism.Commands;
using Prism.Mvvm;
using tikitwo_steam_cleaner.Application.Models;
using tikitwo_steam_cleaner.Application.Services;
using tikitwo_steam_cleaner.Flex;

namespace tikitwo_steam_cleaner.WPF.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly ISizeService _sizeService;
        private readonly ISteamFolderService _steamFolderService;

        public MainViewModel() : this(Di.Resolve<ISteamFolderService>(), Di.Resolve<ISizeService>()) {}

        private MainViewModel(ISteamFolderService steamFolderService, ISizeService sizeService)
        {
            _steamFolderService = steamFolderService;
            _sizeService = sizeService;

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

        public int PackagesFound => FoldersToDelete.Count;

        public ObservableCollection<string> FoldersToSearch {get;}
        public ObservableCollection<RedistItem> FoldersToDelete {get;}
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
                System.Windows.Application.Current.Dispatcher.Invoke(() => FoldersToDelete.Clear());

                var folders = _steamFolderService.Search(FoldersToSearch.ToList()).OrderBy(x => x.Path);

                System.Windows.Application.Current.Dispatcher.Invoke(() => FoldersToDelete.AddRange(folders));
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

                if(!itemsToDelete.Any())
                {
                    FlexibleMessageBox.Show("You must select some files or folders to delete!",
                        "No files or folders to delete.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                var deletedFolders = _steamFolderService.Delete(itemsToDelete);

                if(!deletedFolders.Any())
                {
                    FlexibleMessageBox.Show("None of your selected items were deleted!",
                        "No files or folders deleted.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                var totalSaved = 0L;
                var builder = new StringBuilder("Files/Folders deleted:\r\n\r\n");

                deletedFolders.ForEach(x =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => FoldersToDelete.Remove(x));

                    totalSaved += x.SizeInBytes;

                    builder.AppendLine(x.Path);
                });

                var size = _sizeService.GetDisplaySize(totalSaved);

                FlexibleMessageBox.Show($"You saved {size}!\r\n\r\n{builder}",
                    "Files or folders were deleted.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            });
        }

        private bool CanDeletePackages()
        {
            return CanUseControls && FoldersToDelete.Any() && FoldersToDelete.Any(x => x.Selected);
        }
        #endregion

        #endregion
    }
}