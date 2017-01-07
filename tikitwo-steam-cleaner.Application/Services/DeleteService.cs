using System.IO;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface IDeleteService
    {
        bool DeleteFile(string path);
        bool DeleteFolder(string path);
    }

    public class DeleteService : IDeleteService
    {
        #region IDeleteService Members
        public bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteFolder(string path)
        {
            try
            {
                Directory.Delete(path);

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}