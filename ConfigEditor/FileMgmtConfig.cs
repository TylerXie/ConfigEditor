namespace ConfigEditor
{
    public class FileMgmtConfig : BaseConfig
    {
        public int LoadNumber { get; set; } = 0;
        public int SaveNumber { get; set; } = 0;
        public int UpdateNumber { get; set; } = 0;
        public DateTime LoadTime { get; set; } = new DateTime(1970, 1, 1);
        public DateTime SaveTime { get; set; } = new DateTime(1970, 1, 1);
        public FileMgmtAction FileAction { get; set; } = FileMgmtAction.Load;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }

    public enum FileMgmtAction
    {
        Load,
        Save,
        Update
    }
}