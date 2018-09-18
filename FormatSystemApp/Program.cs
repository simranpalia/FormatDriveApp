using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace FormatSystemApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Format Drive Sample");
            Console.WriteLine("Enter Drive Label (C,D,E etc) :");

            char drive = Console.ReadLine()[0];

            Console.WriteLine("Enter Drive Name :");
            string driveName = Console.ReadLine();

            Console.WriteLine("Starting format....");

            try
            {
                var r = FormatData.FormatDrive(drive, driveName);

                if (r)
                    Console.WriteLine("Formatting completed.");
                else
                    Console.WriteLine("Error occur while formatting.");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.ReadLine();
        }

        public static class FormatData
        {
            public static bool SetLabel(char driveLetter, string label = "")
            {
                if (!Char.IsLetter(driveLetter))
                {
                    return false;
                }
                if (label == null)
                {
                    label = "";
                }

                try
                {
                    DriveInfo di = DriveInfo.GetDrives()
                                            .Where(d => d.Name.StartsWith(driveLetter.ToString()))
                                            .FirstOrDefault();
                    di.VolumeLabel = label;
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public static bool FormatDrive(char driveLetter, string label = "", string fileSystem = "NTFS", bool quickFormat = true, bool enableCompression = false, int? clusterSize = null)
            {
                return FormatDrive_CommandLine(driveLetter, label, fileSystem, quickFormat, enableCompression, clusterSize);
            }

            public static bool FormatDrive_CommandLine(char driveLetter, string label = "", string fileSystem = "NTFS", bool quickFormat = true, bool enableCompression = false, int? clusterSize = null)
            {
                if (!Char.IsLetter(driveLetter) ||
                    !IsFileSystemValid(fileSystem))
                {
                    return false;
                }

                bool success = false;
                string drive = driveLetter + ":";
                try
                {
                    var di = new DriveInfo(drive);
                    var psi = new ProcessStartInfo();
                    psi.FileName = "format.com";
                    psi.WorkingDirectory = Environment.SystemDirectory;
                    psi.Arguments = "/FS:" + fileSystem +
                                                 " /Y" +
                                                 " /V:" + label +
                                                 (quickFormat ? " /Q" : "") +
                                                 ((fileSystem == "NTFS" && enableCompression) ? " /C" : "") +
                                                 (clusterSize.HasValue ? " /A:" + clusterSize.Value : "") +
                                                 " " + drive;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardInput = true;
                    var formatProcess = Process.Start(psi);
                    var swStandardInput = formatProcess.StandardInput;
                    swStandardInput.WriteLine();
                    formatProcess.WaitForExit();
                    success = true;
                }
                catch (Exception) { }
                return success;
            }

            public static bool IsFileSystemValid(string fileSystem)
            {
                #region args check

                if (fileSystem == null)
                {
                    return false;
                }

                #endregion
                switch (fileSystem)
                {
                    case "FAT":
                    case "FAT32":
                    case "EXFAT":
                    case "NTFS":
                    case "UDF":
                        return true;
                    default:
                        return false;
                }
            }

        }
    }
}
