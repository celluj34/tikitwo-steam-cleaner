using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace tikitwo_steam_cleaner.Application.Services
{
    public interface IApplicationSettings
    {
        NameValueCollection Settings {get;}
        List<string> ProgramFileLocations {get;}
        List<KeyValuePair<string, string>> RedistFolders {get;}
        List<KeyValuePair<string, string>> RedistFiles {get;}
        string SteamFolder {get;}
    }

    public class ApplicationSettings : IApplicationSettings
    {
        #region IApplicationSettings Members
        public NameValueCollection Settings => ConfigurationManager.AppSettings;

        public List<string> ProgramFileLocations => new List<string> {Settings["Program Files x86"], Settings["Program Files x64"], string.Empty};

        public List<KeyValuePair<string, string>> RedistFolders => GetSection("RedistFolders");

        public List<KeyValuePair<string, string>> RedistFiles => GetSection("RedistFiles");

        public string SteamFolder => Settings["SteamFolder"];
        #endregion

        private List<KeyValuePair<string, string>> GetSection(string sectionName)
        {
            var section = (NameValueCollection)ConfigurationManager.GetSection(sectionName);

            return section.AllKeys.Select(x => new KeyValuePair<string, string>(x, section[x])).ToList();
        }
    }
}