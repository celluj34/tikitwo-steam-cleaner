using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        string TryGetSteamFolder();

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
        Task<List<FolderThing>> Search(ObservableCollection<string> foldersToSearch);
    }

    public class SteamFolderService : ISteamFolderService
    {
        private const string SteamFolderName = "Steam";
        private readonly ILogicalDriveService _logicalDriveService;
        private readonly List<string> _possibleFolders = new List<string> {"Program Files (x86)", "Program Files", ""};

        public SteamFolderService(ILogicalDriveService logicalDriveService)
        {
            _logicalDriveService = logicalDriveService;
        }

        #region ISteamFolderService Members
        public string TryGetSteamFolder()
        {
            var logicalDrives = _logicalDriveService.GetLogicalDrives();

            var steamFolder =
                logicalDrives.SelectMany(drive => _possibleFolders, (drive, folder) => Path.Combine(drive, folder, SteamFolderName))
                             .FirstOrDefault(Directory.Exists);

            return steamFolder;
        }

        public string GetFolder()
        {
            var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();

            return result == DialogResult.OK ? fbd.SelectedPath : null;
        }

        public Task<List<FolderThing>> Search(ObservableCollection<string> foldersToSearch)
        {
            return
                Task.Run(
                         () =>
                         new List<FolderThing>
                         {
                             new FolderThing {Path = "asdaSD", Selected = false, Size = "asdasda", Type = "12e12edad"},
                             new FolderThing {Path = "576u6y", Selected = false, Size = "asdasda", Type = "p90p08p7"},
                             new FolderThing {Path = "908o7", Selected = true, Size = "12d1d2", Type = "hhuktykj"}
                         });
        }
        #endregion
    }
}