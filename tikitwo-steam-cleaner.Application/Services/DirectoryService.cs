﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface IDirectoryService
    {
        bool Exists(string folder);
        List<string> GetDirectories(string folder);
        long GetFolderSize(string folder);
        List<string> EnumerateFiles(string folder);
    }

    public class DirectoryService : IDirectoryService
    {
        #region IDirectoryService Members
        public bool Exists(string folder)
        {
            return Directory.Exists(folder);
        }

        public List<string> GetDirectories(string folder)
        {
            try
            {
                var topLevelFolder = Directory.EnumerateDirectories(folder);

                var subFolders = Directory.EnumerateDirectories(folder).SelectMany(GetDirectories);

                return topLevelFolder.Concat(subFolders).ToList();
            }
            catch
            {
                return Enumerable.Empty<string>().ToList();
            }
        }

        public long GetFolderSize(string folder)
        {
            return EnumerateFiles(folder).Select(y => new FileInfo(y)).Select(z => z.Length).DefaultIfEmpty(0).Sum();
        }

        public List<string> EnumerateFiles(string folder)
        {
            try
            {
                var topLevelFiles = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories);

                return topLevelFiles.ToList();
            }
            catch
            {
                return Enumerable.Empty<string>().ToList();
            }
        }

        //public List<string> EnumerateFiles(string folder)
        //{
        //    try
        //    {
        //        var topLevelFiles = Directory.EnumerateFiles(folder);

        //        var subFolderFiles = Directory.EnumerateDirectories(folder).SelectMany(EnumerateFiles);

        //        return topLevelFiles.Concat(subFolderFiles).ToList();
        //    }
        //    catch
        //    {
        //        return Enumerable.Empty<string>().ToList();
        //    }
        //}
        #endregion
    }
}