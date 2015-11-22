using System;
using System.IO;
using System.Linq;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface ISizeService
    {
        string GetDisplaySize(long sizeInBytes);
        long GetFileSize(string filePath);
        long GetFolderSize(string folder);
    }

    public class SizeService : ISizeService
    {
        private readonly string[] _sizeSuffixes = {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

        #region ISizeService Members
        public string GetDisplaySize(long sizeInBytes)
        {
            if(sizeInBytes == 0)
            {
                return "0 B";
            }

            var magnitude = (int)Math.Log(sizeInBytes, 1024);
            var adjustedSize = (decimal)sizeInBytes / (1L << (magnitude * 10));

            return $"{adjustedSize:N2} {_sizeSuffixes[magnitude]}";
        }

        public long GetFileSize(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);

                return fileInfo.Length;
            }
            catch
            {
                return 0;
            }
        }

        public long GetFolderSize(string folder)
        {
            return Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Select(y => new FileInfo(y)).Select(z => z.Length).DefaultIfEmpty(0).Sum();
        }
        #endregion
    }
}