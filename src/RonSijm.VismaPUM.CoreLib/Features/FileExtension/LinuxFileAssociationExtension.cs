using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RonSijm.VismaPUM.CoreLib.Features.FileExtension;

public static class LinuxFileAssociationExtension
{
    public static void CreateLinuxFileExtensionAssociation(this string extension, string programName, string fileTypeDescription, string execCommand = "\"%f\"")
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        var desktopFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "applications", $"{programName}.desktop");
        var mimeFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "mime", "packages", $"{programName}-mime.xml");

        var applicationFilePath = Process.GetCurrentProcess().MainModule?.FileName;

        if (string.IsNullOrEmpty(applicationFilePath))
        {
            return;
        }

        var madeChanges = false;
        madeChanges |= CreateDesktopFile(desktopFilePath, programName, applicationFilePath, fileTypeDescription, execCommand);
        madeChanges |= CreateMimeFile(mimeFilePath, programName, extension);

        if (madeChanges)
        {
            UpdateDatabases();
        }
    }

    private static bool CreateDesktopFile(string desktopFilePath, string programName, string execPath, string description, string execCommand)
    {
        var desktopFileContent = $"""
                                  [Desktop Entry]
                                  Name={programName}
                                  Exec={execPath} {execCommand}
                                  MimeType=application/x-{programName};
                                  Type=Application
                                  Terminal=false
                                  Comment={description}
                                  Icon=application-x-executable

                                  """;

        Directory.CreateDirectory(Path.GetDirectoryName(desktopFilePath) ?? string.Empty);
        File.WriteAllText(desktopFilePath, desktopFileContent);

        return true;
    }

    private static bool CreateMimeFile(string mimeFilePath, string programName, string extension)
    {
        var mimeFileContent = $"""
                               <?xml version="1.0" encoding="UTF-8"?>
                               <mime-info xmlns="http://www.freedesktop.org/standards/shared-mime-info">
                                   <mime-type type="application/x-{programName}">
                                       <comment>{programName} File</comment>
                                       <glob pattern="*.{extension}"/>
                                   </mime-type>
                               </mime-info>

                               """;

        Directory.CreateDirectory(Path.GetDirectoryName(mimeFilePath) ?? string.Empty);
        File.WriteAllText(mimeFilePath, mimeFileContent);

        return true;
    }

    private static void UpdateDatabases()
    {
        var mimeUpdateProcess = new ProcessStartInfo
        {
            FileName = "update-mime-database",
            Arguments = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "mime"),
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        Process.Start(mimeUpdateProcess)?.WaitForExit();

        var desktopUpdateProcess = new ProcessStartInfo
        {
            FileName = "update-desktop-database",
            Arguments = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "applications"),
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process.Start(desktopUpdateProcess)?.WaitForExit();
    }
}