using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigEditor
{
    public class GeneralConfig
    {
        public int  AppSection { get; set; } = 0;
        public string  CustomerName { get; set; } = string.Empty;
        public List<DatabaseMgmtConfig> DatabaseMgmtConfigs { get; set; } = new List<DatabaseMgmtConfig>();
        public List<FileMgmtConfig> FileMgmtConfigs { get; set; } = new List<FileMgmtConfig>();
        public List<AppLoadConfig> AppLoadConfigs { get; set; } = new List<AppLoadConfig>();
        public List<AppWriteConfig> AppWriteConfigs { get; set; } = new List<AppWriteConfig>();
    }
}
