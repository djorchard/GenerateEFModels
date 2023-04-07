using System.Reflection;
using static System.Environment;

//  DanApp
//  Details about your app

//  ver 1.05 11/07/2022 ReSharper recommended optimizations
//  ver 1.04 17/06/2022 Added User Roaming data folder
//  ver 1.03 17/06/2022 Moved Web App methods to their own file, and made this partial
//  ver 1.02 12/06/2022 Added method to get SQL Connection string
//  ver 1.01 03/06/2022 Added DataFolder property, changed methods to properties
//  ver 1.00 28/05/2022 Initial version

namespace Dan;

internal static partial class App
{
    public static string Folder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

    public static string Version
    {
        get
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;

            return ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "0";
        }
    }

    public static string Name { get; set; } = Assembly.GetCallingAssembly().GetName().Name ?? "Unnamed Application";

    public static string ShortName => Name.Replace(" ", "");


    //User Roaming data folder
    //C:\Users\%username%\AppData\Roaming\Dan\%appName%
    public static string UserRoamingDataFolder =>
        Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), "Dan", ShortName);

    //Machine Wide data folder
    //C:\ProgramData\Dan\%appName%
    public static string DataFolder => Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), "Dan", ShortName);

    public static void CreateDataFolder()
    {
        if (!Directory.Exists(DataFolder)) { Directory.CreateDirectory(DataFolder); }
    }

    public static void CreateUserRoamingDataFolder()
    {
        if (!Directory.Exists(UserRoamingDataFolder)) { Directory.CreateDirectory(UserRoamingDataFolder); }
    }
}
