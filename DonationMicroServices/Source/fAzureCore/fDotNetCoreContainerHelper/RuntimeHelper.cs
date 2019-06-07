using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;

namespace fDotNetCoreContainerHelper
{
    public class RuntimeHelper
    {
        private static string ListToString(List<string> list)
        {
            var s = new StringBuilder();
            foreach (var l in list)
                s.Append(l).Append(" ");
            return s.ToString().TrimEnd();
        }

        public static int GetCommandLineParameterInt(string name, string[] args)
        {
            var v = GetCommandLineParameterString(name, args);
            int i;
            if (int.TryParse(v, out i))
                return i;
            throw new InvalidProgramException("Cannot find parameter:{name} in command line or environment");
        }

        public static bool GetCommandLineParameterBool(string name, string[] args)
        {
            var v = GetCommandLineParameterString(name, args);
            bool i;
            if (bool.TryParse(v, out i))
                return i;
            throw new InvalidProgramException("Cannot find parameter:{name} in command line or environment");
        }

        public static string GetCommandLineParameterString(string name, string[] args)
        {
            var envName = name;
            while(envName.StartsWith("-"))
                envName = envName.Substring(1);

            var v = Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrEmpty(v))
                return v;
            for(var i=0; i<args.Length; i++)
            {
                if(args[i] == name && i+1 < args.Length)
                {
                    return args[i+1];
                }
            }
            return null;
        }


        public static Dictionary<string, object> GetContextInformationDictionary()
        {
            var d = new Dictionary<string, object>();
            d.Add("CommandLine", Environment.CommandLine);
            d.Add("CurrentDirectory", Environment.CurrentDirectory);
            d.Add("GetCommandLineArgs", ListToString(Environment.GetCommandLineArgs().ToList()));
            d.Add("Is64BitOperatingSystem", Environment.Is64BitOperatingSystem);
            d.Add("Is64BitProcess", Environment.Is64BitProcess);
            d.Add("MachineName", Environment.MachineName);
            d.Add("UserDomainName", Environment.UserDomainName);
            d.Add("UserName", Environment.UserName);
            d.Add("Common Language Runtime Version", Environment.Version);
            d.Add("OSVersion", Environment.OSVersion);
            d.Add("SystemDirectory", Environment.SystemDirectory);
            d.Add("NewLine.Length", Environment.NewLine.Length);
            d.Add("IsRunningContainerMode", IsRunningContainerMode());

            foreach (DictionaryEntry e in Environment.GetEnvironmentVariables())
            {
                d.Add(e.Key.ToString(), e.Value);
            }
            return d;
        }

        public static string GetContextInformation(bool json = false)
        {
            var d = GetContextInformationDictionary();
            
            if(json)
            {
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(d, Newtonsoft.Json.Formatting.Indented);
                return jsonContent;
            }
            else
            {
                var s = new StringBuilder();
                foreach(var e in d)
                    s.Append($"{e.Key}: {e.Value}").AppendLine();
                return s.ToString();
            }
        }

        public static bool IsRunningContainerMode()
        {
            return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        }

        public static string GetAppVersion()
        {
            return Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        public static string _AppPath = null;

        // Allow to set the path, needed for web app versus console app
        public static void SetAppPath(string path)
        {
            _AppPath = path;
        }

        public static string GetAppPath()
        {
            if (_AppPath != null)
                return _AppPath;
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetAppSettingsJsonFile()
        {
            return GetAppFilePath("appsettings.json");
        }

        public static string GetAppFilePath(string file)
        {
            return Path.Combine(GetAppPath(), file);
        }

        public static string GetAppFolderPath(string folder)
        {
            return Path.Combine(GetAppPath(), folder);
        }

        public static string GetMyDocumentsPath()
        {
            // /root in Linux
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 
        }

        const string APP_SETTING_JSON_FILE_NAME = "appsettings.json";

        private static IConfigurationRoot _configurationRoot = null;
        

        public static IConfigurationRoot BuildAppSettingsJsonConfiguration()
        {
            if (_configurationRoot == null)
            {
                var p = RuntimeHelper.GetAppPath();
                // Console.WriteLine($"Reading configuration {RuntimeHelper.GetAppSettingsJsonFile()}");
                var builder = new ConfigurationBuilder()
                                .SetBasePath(RuntimeHelper.GetAppPath())
                                .AddJsonFile(APP_SETTING_JSON_FILE_NAME, optional: true, reloadOnChange: true);
                _configurationRoot = builder.Build();
            }
            return _configurationRoot;
        }

        public static string GetAppSettings(string name)
        {
            return BuildAppSettingsJsonConfiguration()[name];
        }

        public static void InfiniteLoop(int max = 10000)
        {
            var loopIndex = 0;
            while (true)
            {
                Console.WriteLine($"InfiniteLoop {loopIndex++}");
                System.Threading.Tasks.Task.Delay(1000 * 10).Wait();
                if (loopIndex > max)
                    break;
            }
        }
    }
}
