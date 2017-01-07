namespace tikitwo_steam_cleaner.Application.Models
{
    public class RedistItem
    {
        public bool Selected {get;set;}
        public string Path {get;set;}
        public long SizeInBytes {get;set;}
        public string DisplaySize {get;set;}
        public string DisplayType {get;set;}
        public ItemTypeEnum ItemType {get;set;}
        public bool Deleted {get;set;}
    }
}