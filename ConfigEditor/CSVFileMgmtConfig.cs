using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigEditor
{
    public class CSVFileMgmtConfig: FileMgmtConfig
    {
        public int RowNumber { get; set; } = 0;
        public string CodeColumnName { get; set; } = string.Empty;
        public string NameColumnName { get; set; } = string.Empty;
        public string DescriptionColumnName { get; set; } = string.Empty;

    }
}
