using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface IDirectoryService
    {
        bool Exists(string folder);
        List<string> GetDirectories(string folder, SearchOption searchOption);
        long GetFolderSize(string folder);
    }

    public class DirectoryService : IDirectoryService
    {
        #region IDirectoryService Members
        public bool Exists(string folder)
        {
            return Directory.Exists(folder);
        }

        public List<string> GetDirectories(string folder, SearchOption searchOption)
        {
            return Directory.GetDirectories(folder, "*", searchOption).ToList();
        }

        public long GetFolderSize(string folder)
        {
            return Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).Select(y => new FileInfo(y)).Select(z => z.Length).DefaultIfEmpty(0).Sum();
        }
        #endregion
    }
}