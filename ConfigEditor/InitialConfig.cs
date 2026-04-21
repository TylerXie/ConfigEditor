namespace ConfigEditor
{
    /// <summary>
    /// Special configuration class that represents the application's initial settings.
    /// This config item is always present and cannot be removed from the application.
    /// </summary>
    public class InitialConfig : BaseConfig
    {
        public string Description { get; set; } = "Application Initial Configuration";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Environment { get; set; } = "Development";

        public InitialConfig()
        {
            AppSection = -1; // Special section for initial config
            AppName = "InitialConfig";
            AppVersion = "1.0";
        }
    }
}
