using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using tikitwo_steam_cleaner.Application.Models;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface IRedistFileService
    {
        List<RedistItem> GetRedistFolders(List<string> allFolders);
        List<RedistItem> GetRedistFiles(List<string> allFolders, List<RedistItem> redistFolders);
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

        private static void RemoveNestedFolders(List<string> redistFolders)
        {
            for(var i = 0; i < redistFolders.Count; i++)
            {
                var left = redistFolders.ElementAt(i);

                for(var j = i + 1; j < redistFolders.Count; j++)
                {
                    var right = redistFolders.ElementAt(j);

                    if(!right.StartsWith(left))
                    {
                        continue;
                    }

                    redistFolders.RemoveAt(j);

                    --j;
                }
            }
        }

        #region Implementation of IRedistFileService
        public List<RedistItem> GetRedistFolders(List<string> allFolders)
        {
            var redistFolders = allFolders.Where(FolderIsRedistFolder).ToList();

            RemoveNestedFolders(redistFolders);

            return
                redistFolders.AsParallel()
                             .Select(x => new RedistItem {Path = x, Selected = true, Type = "Folder", Size = _directoryService.GetFolderSize(x)})
                             .ToList();
        }

        public List<RedistItem> GetRedistFiles(List<string> allFolders, List<RedistItem> redistFolders)
        {
            var redistFiles =
                allFolders.AsParallel()
                          .Where(allFolder => !redistFolders.Any(redistFolder => allFolder.StartsWith(redistFolder.Path)))
                          .Select(GetRedistFilesInFolder)
                          .SelectMany(x => x)
                          .Select(y => new RedistItem {Selected = true, Path = y, Type = "File", Size = new FileInfo(y).Length})
                          .ToList();

            return redistFiles;
        }
        #endregion
    }
}