using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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
        string GetSteamFolder();
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
                logicalDrives.SelectMany(drive => _possibleFolders, (drive, thing2) => Path.Combine(drive, thing2, SteamFolderName))
                             .FirstOrDefault(Directory.Exists);

            return steamFolder;
        }

        public string GetSteamFolder()
        {
            var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();

            return result == DialogResult.OK ? fbd.SelectedPath : null;
        }
        #endregion
    }
}