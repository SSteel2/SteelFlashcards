namespace SteelFlashcards
{
    static class UserConfiguration
    {
        public static string? LastUsedDictionary 
        { 
            get { return Windows.Storage.ApplicationData.Current.LocalSettings.Values["LastUsedDictionary"] as string; }
            set { Windows.Storage.ApplicationData.Current.LocalSettings.Values["LastUsedDictionary"] = value; }
        }
    }
}
