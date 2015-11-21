using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using tikitwo_steam_cleaner.Application.Models;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface IRedistFileService
    {
        List<RedistItem> GetRedistFolders(List<string> allSubFolders);
        List<RedistItem> GetRedistFiles(List<string> allSubFolders, List<RedistItem> redistFolders);
    }

    public class RedistFileService : IRedistFileService
    {
        private readonly IDirectoryService _directoryService;
        private readonly IReadOnlyCollection<Regex> _redistFiles;
        private readonly IReadOnlyCollection<Regex> _redistFolders;

        public RedistFileService(IApplicationSettings applicationSettings, IDirectoryService directoryService)
        {
            _directoryService = directoryService;

            const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;

            _redistFolders = applicationSettings.RedistFolders.Select(x => new Regex(x.Key, regexOptions)).ToList();
            _redistFiles = applicationSettings.RedistFiles.Select(x => new Regex(x.Key, regexOptions)).ToList();
        }

        private bool FolderIsRedistFolder(string folder)
        {
            return _redistFolders.Any(x => x.IsMatch(folder));
        }

        private List<string> GetRedistFilesInFolder(string folder)
        {
            return _directoryService.EnumerateFiles(folder).Where(x => _redistFiles.Any(y => y.IsMatch(x))).ToList();
        }

        #region Implementation of IRedistFileService
        public List<RedistItem> GetRedistFolders(List<string> allSubFolders)
        {
            var redistFolders =
                allSubFolders.AsParallel()
                             .Where(FolderIsRedistFolder)
                             .Select(x => new RedistItem {Path = x, Selected = true, Type = "Folder", Size = _directoryService.GetFolderSize(x)})
                             .Where(y => y.Size > 0)
                             .ToList();

            return redistFolders;
        }

        public List<RedistItem> GetRedistFiles(List<string> allSubFolders, List<RedistItem> redistFolders)
        {
            var redistFolderPaths = redistFolders.Select(y => y.Path).ToList();

            var redistFiles =
                allSubFolders.AsParallel()
                             .Where(x => !redistFolderPaths.Contains(x))
                             .Select(GetRedistFilesInFolder)
                             .SelectMany(x => x)
                             .Select(y => new RedistItem {Selected = true, Path = y, Type = "File", Size = new FileInfo(y).Length})
                             .ToList();

            return redistFiles;
        }
        #endregion
    }
}