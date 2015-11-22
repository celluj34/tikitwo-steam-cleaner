using System.Collections.Generic;
using System.IO;
using System.Linq;
using tikitwo_steam_cleaner.Application.Models;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface IDirectoryService
    {
        bool FolderExists(string folder);
        List<string> GetDirectories(string folder);
        List<string> EnumerateFiles(string folder);
        bool Delete(RedistItem itemToDelete);
    }

    public class DirectoryService : IDirectoryService
    {
        #region IDirectoryService Members
        public bool FolderExists(string folder)
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

        public bool Delete(RedistItem itemToDelete)
        {
            switch(itemToDelete.ItemType)
            {
                case ItemTypeEnum.Folder:
                    return DeleteFolder(itemToDelete.Path);

                case ItemTypeEnum.File:
                    return DeleteFile(itemToDelete.Path);

                default:
                    return false;
            }
        }
        #endregion

        private bool DeleteFolder(string folderToDelete)
        {
            try
            {
                var files = Directory.GetFiles(folderToDelete);

                var success = files.Aggregate(true, (current, file) => current & DeleteFile(file));

                var folders = Directory.GetDirectories(folderToDelete);

                success = folders.Aggregate(success, (current, folder) => current & DeleteFolder(folder));

                Directory.Delete(folderToDelete);

                return success;
            }
            catch
            {
                return false;
            }
        }

        private bool DeleteFile(string fileToDelete)
        {
            try
            {
                File.Delete(fileToDelete);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}