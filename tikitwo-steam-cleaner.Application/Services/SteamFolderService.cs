using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using tikitwo_steam_cleaner.Application.Models;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface ISteamFolderService
    {
        /// <summary>
        ///     Tries to find the location of the users Steam folder based on a few known locations.
        /// </summary>
        /// <returns>String representation of the path to the folder, if found. Null otherwise.</returns>
        List<string> TryGetSteamFolder();

        /// <summary>
        ///     Uses a FolderBrowserDialog to let the user select their own folder.
        /// </summary>
        /// <returns>String representation of the path to the folder, if selected. Null otherwise.</returns>
        string GetFolder();

        /// <summary>
        ///     Search the given folders for duplicate packages.
        /// </summary>
        /// <param name="foldersToSearch"></param>
        /// <returns>A list of folders which contains duplicate packages.</returns>
        List<RedistItem> Search(List<string> foldersToSearch);

        /// <summary>
        ///     Deletes all of the items.
        /// </summary>
        /// <param name="itemsToDelete"></param>
        /// <returns>A list of items that have been successfully deleted.</returns>
        List<RedistItem> Delete(List<RedistItem> itemsToDelete);
    }

    public class SteamFolderService : ISteamFolderService
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly IDeleteService _deleteService;
        private readonly IDirectoryService _directoryService;
        private readonly ILogicalDriveService _logicalDriveService;
        private readonly IRedistFileService _redistFileService;

        public SteamFolderService(IApplicationSettings applicationSettings,
                                  ILogicalDriveService logicalDriveService,
                                  IRedistFileService redistFileService,
                                  IDirectoryService directoryService,
                                  IDeleteService deleteService)
        {
            _applicationSettings = applicationSettings;
            _logicalDriveService = logicalDriveService;
            _redistFileService = redistFileService;
            _directoryService = directoryService;
            _deleteService = deleteService;
        }

        #region ISteamFolderService Members
        public List<string> TryGetSteamFolder()
        {
            var logicalDrives = _logicalDriveService.GetLogicalDrives();

            //cross-join all logical drives with all programFileLocations
            var steamFolder =
                logicalDrives.SelectMany(drive => _applicationSettings.ProgramFileLocations,
                                 (drive, folder) => Path.Combine(drive, folder, _applicationSettings.SteamFolder))
                             .Where(_directoryService.FolderExists)
                             .ToList();

            return steamFolder;
        }

        public string GetFolder()
        {
            var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();

            return result == DialogResult.OK ? fbd.SelectedPath : null;
        }

        public List<RedistItem> Search(List<string> foldersToSearch)
        {
            return
                foldersToSearch.Distinct()
                               .AsParallel()
                               .Where(x => _directoryService.FolderExists(x))
                               .SelectMany(GetRedistItemsForFolder)
                               .ToList();
        }

        public List<RedistItem> Delete(List<RedistItem> itemsToDelete)
        {
            return itemsToDelete.AsParallel().Select(Delete).Where(x => x.Deleted).ToList();
        }
        #endregion

        private RedistItem Delete(RedistItem item)
        {
            switch(item.ItemType)
            {
                case ItemTypeEnum.Folder:
                    item.Deleted = _deleteService.DeleteFolder(item.Path);
                    break;

                case ItemTypeEnum.File:
                    item.Deleted = _deleteService.DeleteFile(item.Path);
                    break;
            }

            return item;
        }

        private List<RedistItem> GetRedistItemsForFolder(string folder)
        {
            var allFolders = _directoryService.GetDirectories(folder);

            var redistFolders = _redistFileService.GetRedistFolders(allFolders);

            var redistFiles = _redistFileService.GetRedistFiles(allFolders, redistFolders);

            return redistFolders.Concat(redistFiles).Where(x => x.SizeInBytes > 0).ToList();
        }
    }
}