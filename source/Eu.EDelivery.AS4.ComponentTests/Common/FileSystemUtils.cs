﻿using System.IO;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    internal static class FileSystemUtils
    {
        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            if (Directory.Exists(sourceDirName) == false)
            {
                throw new DirectoryNotFoundException($"The {sourceDirName} directory can not be found.");
            }

            if (Directory.Exists(destDirName) == false)
            {
                Directory.CreateDirectory(destDirName);
            }

            var files = Directory.GetFiles(sourceDirName);

            foreach (string fileName in files)
            {
                File.Copy(fileName, Path.Combine(destDirName, Path.GetFileName(fileName)), true);
            }
        }

        public static void ClearDirectory(string directoryName)
        {
            if (Directory.Exists(directoryName) == false)
            {
                throw new DirectoryNotFoundException($"The {directoryName} directory can not be found.");
            }

            var subDirectories = Directory.GetDirectories(directoryName);
            var files = Directory.GetFiles(directoryName);

            foreach (var subdirectory in subDirectories)
            {
                ClearDirectory(subdirectory);
            }

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
