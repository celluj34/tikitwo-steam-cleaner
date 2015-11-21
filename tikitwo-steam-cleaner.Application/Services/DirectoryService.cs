using System.Collections.Generic;
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
                var topLevelFolders = Directory.EnumerateDirectories(folder);

                var subFolders = Directory.EnumerateDirectories(folder).SelectMany(GetDirectories);

                return topLevelFolders.Concat(subFolders).ToList();
            }
            catch
            {
                return Enumerable.Empty<string>().ToList();
            }
        }

        public long GetFolderSize(string folder)
        {
            return Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Select(y => new FileInfo(y)).Select(z => z.Length).DefaultIfEmpty(0).Sum();
        }

        public List<string> EnumerateFiles(string folder)
        {
            try
            {
                return Directory.EnumerateFiles(folder).ToList();
            }
            catch
            {
                return Enumerable.Empty<string>().ToList();
            }
        }
        #endregion
    }
}