using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface ILogicalDriveService
    {
        /// <summary>
        /// Returns all logical drives connected to the computer. e.g., C:\, D:\, etc.
        /// </summary>
        /// <returns>A list of strings of all connected drives.</returns>
        List<string> GetLogicalDrives();
    }

    public class LogicalDriveService : ILogicalDriveService
    {
        #region ILogicalDriveService Members
        public List<string> GetLogicalDrives()
        {
            return Directory.GetLogicalDrives().ToList();
        }
        #endregion
    }
}