using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        ///     Deletes all of the folders.
        /// </summary>
        /// <param name="foldersToDelete"></param>
        /// <returns>A list of folders that have been successfully deleted.</returns>
        List<RedistItem> Delete(List<RedistItem> foldersToDelete);
    }

    public class SteamFolderService : ISteamFolderService
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly IDirectoryService _directoryService;
        private readonly ILogicalDriveService _logicalDriveService;
        private readonly IRedistFileService _redistFileService;

        public SteamFolderService(IApplicationSettings applicationSettings,
                                  ILogicalDriveService logicalDriveService,
                                  IRedistFileService redistFileService,
                                  IDirectoryService directoryService)
        {
            _applicationSettings = applicationSettings;
            _logicalDriveService = logicalDriveService;
            _redistFileService = redistFileService;
            _directoryService = directoryService;
        }

        #region ISteamFolderService Members
        public List<string> TryGetSteamFolder()
        {
            var logicalDrives = _logicalDriveService.GetLogicalDrives();

            var programFileLocations = _applicationSettings.ProgramFileLocations;
            var steamFolderName = _applicationSettings.SteamFolder;

            //cross-join all logical drives with all programFileLocations
            var steamFolder =
                logicalDrives.SelectMany(drive => programFileLocations, (drive, folder) => Path.Combine(drive, folder, steamFolderName))
                             .Where(x => _directoryService.Exists(x))
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
            return foldersToSearch.Distinct().AsParallel().Where(x => _directoryService.Exists(x)).SelectMany(GetRedistItemsForFolder).ToList();
        }

        public List<RedistItem> Delete(List<RedistItem> foldersToDelete)
        {
            var deletedFolders = new List<RedistItem>();

            foreach(var folderToDelete in foldersToDelete)
            {
                try
                {
                    //TODO: actually delete things
                    Thread.Sleep(100);

                    deletedFolders.Add(folderToDelete);
                }
                catch(Exception ex)
                {
                    //catch and do stuff here
                }
            }

            return deletedFolders;
        }
        #endregion

        private List<RedistItem> GetRedistItemsForFolder(string folder)
        {
            var allFolders = _directoryService.GetDirectories(folder);

            var redistFolders = _redistFileService.GetRedistFolders(allFolders);

            var redistFiles = _redistFileService.GetRedistFiles(allFolders, redistFolders);

            return redistFolders.Concat(redistFiles).Where(x => x.Size > 0).ToList();
        }
    }
}