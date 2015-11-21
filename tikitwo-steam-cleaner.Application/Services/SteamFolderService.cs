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
        /// Tries to find the location of the users Steam folder based on a few known locations.
        /// </summary>
        /// <returns>String representation of the path to the folder, if found. Null otherwise.</returns>
        List<string> TryGetSteamFolder();

        /// <summary>
        /// Uses a FolderBrowserDialog to let the user select their own folder.
        /// </summary>
        /// <returns>String representation of the path to the folder, if selected. Null otherwise.</returns>
        string GetFolder();

        /// <summary>
        /// Search the given folders for duplicate packages.
        /// </summary>
        /// <param name="foldersToSearch"></param>
        /// <returns>A list of folders which contains duplicate packages.</returns>
        List<RedistItem> Search(List<string> foldersToSearch);

        /// <summary>
        /// Deletes all of the folders.
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

            //cross-join all logical drives with all _possibleFolders
            var steamFolder =
                logicalDrives.SelectMany(drive => _possibleFolders, (drive, folder) => Path.Combine(drive, folder, SteamFolderName))
                             .Where(Directory.Exists)
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
            return Task.Run(() =>
            {
                var foundFolders = new List<FolderThing>();
                var existingFolders = foldersToSearch.Distinct().Where(Directory.Exists).ToList();

                foreach(var folderToSearch in existingFolders)
                {
                    var directories =
                        Directory.GetDirectories(folderToSearch, "*", SearchOption.AllDirectories)
                                 .Select(
                                         x =>
                                         new
            {
                                             Path = x,
                                             Type = "idk",
                                             Size =
                                             Directory.GetFiles(x, "*.*", SearchOption.AllDirectories)
                                                      .Select(y => new FileInfo(y))
                                                      .Select(z => z.Length)
                                                      .DefaultIfEmpty(0)
                                                      .Sum()
                                         })
                                 .Select(z => new FolderThing {Selected = true, Path = z.Path, Size = $"{((double)z.Size / 1024 / 1024).ToString("N2")} MB"})
                                 .ToList();

                    foundFolders.AddRange(directories);
            }

                return foundFolders;
            });
        }

        public List<RedistItem> Delete(List<RedistItem> foldersToDelete)
        {
            return Task.Run(() =>
            {
                var deletedFolders = new List<FolderThing>();

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
            });
        }
        #endregion
    }
}