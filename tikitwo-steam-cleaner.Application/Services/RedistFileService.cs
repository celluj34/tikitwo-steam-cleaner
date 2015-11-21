using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface IRedistFileService
    {
        bool FolderIsRedistFolder(string folderPath);
        List<string> GetRedistFilesInFolder(string folderPath);
    }

    public class RedistFileService : IRedistFileService
    {
        private readonly IReadOnlyCollection<Regex> _redistFiles;
        private readonly IReadOnlyCollection<Regex> _redistFolders;

        public RedistFileService(IApplicationSettings applicationSettings)
        {
            _redistFolders = applicationSettings.RedistFolders.Select(x => new Regex(x.Key)).ToList();
            _redistFiles = applicationSettings.RedistFiles.Select(x => new Regex(x.Key)).ToList();
        }

        #region Implementation of IRedistFileService
        public bool FolderIsRedistFolder(string folderPath)
        {
            return _redistFolders.Any(x => x.IsMatch(folderPath));
        }

        public List<string> GetRedistFilesInFolder(string folderPath)
        {
            return Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories).Where(x => _redistFiles.Any(y => y.IsMatch(x))).ToList();
        }
        #endregion
    }
}