using System.IO;
using System.Threading;

const uint checkEverySeconds = 5;

while (true)
{
    DriveInfo[] drives = DriveInfo.GetDrives();

    foreach (DriveInfo drive in drives)
    {
        Console.WriteLine(drive);
    }

    Thread.Sleep((int)checkEverySeconds * 1000);
}
