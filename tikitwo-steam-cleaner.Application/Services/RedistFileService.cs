using System;
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
        private static readonly string[] SizeSuffixes = {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
        private readonly IDirectoryService _directoryService;
        private readonly IReadOnlyDictionary<Regex, string> _redistFiles;
        private readonly IReadOnlyDictionary<Regex, string> _redistFolders;

        public RedistFileService(IApplicationSettings applicationSettings, IDirectoryService directoryService)
        {
            _directoryService = directoryService;

            const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;

            _redistFolders = applicationSettings.RedistFolders.ToDictionary(x => new Regex(x.Key, regexOptions), y => y.Value);
            _redistFiles = applicationSettings.RedistFiles.ToDictionary(x => new Regex(x.Key, regexOptions), y => y.Value);
        }

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
            return new RedistItem
            {
                Path = path,
                Selected = true,
                SizeInBytes = size,
                DisplayType = GetDisplayType(path, itemType),
                DisplaySize = GetDisplaySize(size),
                ItemType = itemType
            };
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

        private string GetDisplaySize(long size)
        {
            if(size == 0)
            {
                return "0 B";
            }
            var magnitude = (int)Math.Log(size, 1024);
            var adjustedSize = (decimal)size / (1L << (magnitude * 10));

            return $"{adjustedSize:N2} {SizeSuffixes[magnitude]}";
        }

        #region Implementation of IRedistFileService
        public List<RedistItem> GetRedistFolders(List<string> allFolders)
        {
            var redistFolders = allFolders.Where(FolderIsRedistFolder).ToList();

            RemoveNestedFolders(redistFolders);

            return
                redistFolders.AsParallel()
                             .Select(x => new {Path = x, Size = _directoryService.GetFolderSize(x)})
                             .Select(x => GenerateRedistItem(x.Path, x.Size, ItemTypeEnum.Folder))
                             .ToList();
        }

        public List<RedistItem> GetRedistFiles(List<string> allFolders, List<RedistItem> redistFolders)
        {
            var redistFiles =
                allFolders.AsParallel()
                          .Where(allFolder => !redistFolders.Any(redistFolder => allFolder.StartsWith(redistFolder.Path)))
                          .Select(GetRedistFilesInFolder)
                          .SelectMany(x => x)
                          .Select(y => new {Path = y, Size = _directoryService.GetFileSize(y)})
                          .Select(x => GenerateRedistItem(x.Path, x.Size, ItemTypeEnum.File))
                          .ToList();

            return redistFiles;
        }
        #endregion
    }
}