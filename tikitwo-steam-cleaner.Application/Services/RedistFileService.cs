using System.Collections.Generic;
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
        private readonly IReadOnlyDictionary<Regex, string> _redistFiles;
        private readonly IReadOnlyDictionary<Regex, string> _redistFolders;
        private readonly ISizeService _sizeService;

        public RedistFileService(IApplicationSettings applicationSettings, IDirectoryService directoryService, ISizeService sizeService)
        {
            _directoryService = directoryService;
            _sizeService = sizeService;

            const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;

            _redistFolders = applicationSettings.RedistFolders.ToDictionary(x => new Regex(x.Key, regexOptions), y => y.Value);
            _redistFiles = applicationSettings.RedistFiles.ToDictionary(x => new Regex(x.Key, regexOptions), y => y.Value);
        }

        #region IRedistFileService Members
        public List<RedistItem> GetRedistFolders(List<string> allFolders)
        {
            var redistFolders = allFolders.Where(FolderIsRedistFolder).ToList();

            RemoveNestedFolders(redistFolders);

            return
                redistFolders.AsParallel()
                             .Select(x => new {Path = x, Size = _sizeService.GetFolderSize(x)})
                             .Select(x => GenerateRedistItem(x.Path, x.Size, ItemTypeEnum.Folder))
                             .ToList();
        }

        public List<RedistItem> GetRedistFiles(List<string> allFolders, List<RedistItem> redistFolders)
        {
            var redistFiles =
                allFolders.AsParallel()
                          .Where(folder => !redistFolders.Any(redistFolder => folder.StartsWith(redistFolder.Path)))
                          .Select(GetRedistFilesInFolder)
                          .SelectMany(x => x)
                          .Select(y => new {Path = y, Size = _sizeService.GetFileSize(y)})
                          .Select(x => GenerateRedistItem(x.Path, x.Size, ItemTypeEnum.File))
                          .ToList();

            return redistFiles;
        }
        #endregion

        private bool FolderIsRedistFolder(string folder)
        {
            return _redistFolders.Any(x => x.Key.IsMatch(folder));
        }

        private List<string> GetRedistFilesInFolder(string folder)
        {
            return _directoryService.EnumerateFiles(folder).Where(x => _redistFiles.Any(y => y.Key.IsMatch(x))).ToList();
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

        private RedistItem GenerateRedistItem(string path, long size, ItemTypeEnum itemType)
        {
            var displayType = GetDisplayType(path, itemType);
            var displaySize = _sizeService.GetDisplaySize(size);

            return new RedistItem(path, size, displayType, displaySize, itemType);
        }

        private string GetDisplayType(string path, ItemTypeEnum itemType)
        {
            switch(itemType)
            {
                case ItemTypeEnum.Folder:
                    return _redistFolders.First(x => x.Key.IsMatch(path)).Value;

                case ItemTypeEnum.File:
                    return _redistFiles.First(x => x.Key.IsMatch(path)).Value;

                default:
                    return null;
            }
        }
    }
}