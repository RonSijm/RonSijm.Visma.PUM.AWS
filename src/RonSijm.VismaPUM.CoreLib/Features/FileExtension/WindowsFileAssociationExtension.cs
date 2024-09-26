using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

#pragma warning disable CA1416
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
namespace RonSijm.VismaPUM.CoreLib.Features.FileExtension;

public static class WindowsFileAssociationExtension
{
    [DllImport("Shell32.dll")]
    private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

    private const int SHCNE_ASSOCCHANGED = 0x8000000;
    private const int SHCNF_FLUSH = 0x1000;

    public static void CreateWindowsFileExtensionAssociation(this string extension, string programName, string fileTypeDescription, string inputCommand = "\"%1\"")
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var mainModule = Process.GetCurrentProcess().MainModule;

        if (mainModule == null)
        {
            return;
        }

        var applicationFilePath = mainModule.FileName;

        var madeChanges = false;
        madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + extension, programName);
        madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + programName, fileTypeDescription);
        madeChanges |= SetKeyDefaultValue($@"Software\Classes\{programName}\shell\open\command", $"\"{applicationFilePath}\" {inputCommand}");

        if (madeChanges)
        {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
        }
    }

    private static bool SetKeyDefaultValue(string keyPath, string value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(keyPath);

        if (key == null)
        {
            return false;
        }

        if (key.GetValue(null) as string == value)
        {
            return false;
        }

        key.SetValue(null, value);

        return true;
    }
}