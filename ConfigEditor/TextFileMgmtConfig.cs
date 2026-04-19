using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigEditor
{
    public class TextFileMgmtConfig : FileMgmtConfig
    {
        public int CodeStart { get; set; } = 0;
        public int CodeEnd { get; set; } = 15;
        public int NameLength { get; set; } = 50;
        public string Name { get; set; } = string.Empty;
        public int DescriptionLength { get; set; } = 100;
    }
}
