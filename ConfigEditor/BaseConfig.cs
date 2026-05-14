using System.ComponentModel.DataAnnotations;

namespace ConfigEditor
{
    public class BaseConfig
    {
        public int AppSection { get; set; } = 0;
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "AppName can only contain letters or spaces.")]
        public string AppName { get; set; } = string.Empty;
        public string AppVersion { get; set; }= string.Empty;

    }
}