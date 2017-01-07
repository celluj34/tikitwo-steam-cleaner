namespace tikitwo_steam_cleaner.Application.Models
{
    public class RedistItem
    {
        public RedistItem(string path, long size, string displayType, string displaySize, ItemTypeEnum itemType)
        {
            Path = path;
            Selected = true;
            SizeInBytes = size;
            DisplayType = displayType;
            DisplaySize = displaySize;
            ItemType = itemType;
        }

        public bool Selected {get;set;}
        public string Path {get;}
        public long SizeInBytes {get;}
        public string DisplaySize {get;}
        public string DisplayType {get;}
        public ItemTypeEnum ItemType {get;}
        public bool Deleted {get;set;}
    }
}