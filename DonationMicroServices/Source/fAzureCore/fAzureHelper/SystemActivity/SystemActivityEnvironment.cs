using System;
using System.Diagnostics;

namespace fAzureHelper
{
    public class SystemActivityEnvironment
    {
        public string MachineName { get; set; }
        public string UserName { get; set; }
        public SystemActivityType Type { get; set; }
        public string CurrentDirectory;
        public bool Is64BitOperatingSystem;
        public bool Is64BitProcess;
        public string OSVersion;
        public int ProcessorCount;
        public string CommandLine;
        public SystemActivityEnvironment()
        {
            this.MachineName = Environment.MachineName;
            this.UserName = Environment.UserName;
            this.CurrentDirectory = Environment.CurrentDirectory;
            this.Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
            this.Is64BitProcess = Environment.Is64BitProcess;
            this.OSVersion = Environment.OSVersion.VersionString;
            this.ProcessorCount = Environment.ProcessorCount;
            this.CommandLine = Environment.CommandLine;
        }
    }
}
