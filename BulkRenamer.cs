using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace bulk_rename
{
    public class BulkRenamer
    {
        public const string RENAME_FILE_NAME = ".bulk_rename";

        public BulkRenamer(string directory)
        {
            this.directory = directory;

            initialRenameFileState = new List<string>();

            watcher = new FileSystemWatcher(directory, RENAME_FILE_NAME);
            watcher.Changed += RenameFileChanged;
            watcher.Deleted += RenameFileDeleted;
        }

        private string directory;
        private List<string> initialRenameFileState;
        private FileSystemWatcher watcher;

        public void Rename()
        {
            if (!Directory.Exists(directory)) return;

            // list all files and directories in directory
            initialRenameFileState.Clear();
            initialRenameFileState.AddRange(
                Directory
                .EnumerateDirectories(directory)
                .Select(item => Path.GetFileName(item) + Path.DirectorySeparatorChar)
                .Concat(
                    Directory
                    .EnumerateFiles(directory)
                    .Select(item => Path.GetFileName(item))
                )
            );

            // rename file is placed in parent directory
            string renameFile = Path.Combine(directory, RENAME_FILE_NAME);

            // create new rename file
            using (StreamWriter writer = new StreamWriter(renameFile))
                foreach (string file in initialRenameFileState)
                    writer.WriteLine(file);

            // make rename file hidden
            File.SetAttributes(renameFile, File.GetAttributes(renameFile) | FileAttributes.Hidden);

            // watch for changes to rename file
            watcher.EnableRaisingEvents = true;

            // open editor
            string editorPath = FindExecutable("code.cmd");

            ProcessStartInfo pci = new ProcessStartInfo()
            {
                FileName = editorPath,
                Arguments = "\"" + renameFile + "\"",
                CreateNoWindow = true
            };
            Process.Start(pci);
        }

        public void Wait()
        {
            lock (watcher)
                Monitor.Wait(watcher);
        }

        private void RenameFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name != RENAME_FILE_NAME) return;
            if (!File.Exists(e.FullPath)) return;

            // wait for writing to finish
            Thread.Sleep(100);

            // get rename file state after change
            List<string> currentRenameFileState = new List<string>();
            using (StreamReader reader = new StreamReader(WaitForFileStream(e.FullPath, FileMode.Open, FileAccess.Read)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    currentRenameFileState.Add(line);
            }

            // only look for changed lines (not new or deleted lines)
            foreach ((string last, string current) in initialRenameFileState.Zip(currentRenameFileState))
            {
                if (last == current) continue;

                string lastPath = Path.Combine(directory, last);
                string currentPath = Path.Combine(directory, current);

                if (Directory.Exists(lastPath))
                    Directory.Move(lastPath, currentPath);
                else if (File.Exists(lastPath))
                    File.Move(lastPath, currentPath);
            }

            // delete rename file
            try
            {
                File.Delete(e.FullPath);
            }
            catch (IOException) {}
        }

        private void RenameFileDeleted(object sender, FileSystemEventArgs e)
        {
            lock (watcher)
            {
                watcher.Dispose();
                Monitor.PulseAll(watcher);
            }
        }

        private static string FindExecutable(string name)
        {
            string path = Environment.GetEnvironmentVariable("PATH");
            return path
                .Split(';')
                .Select(directory => Path.Combine(directory, name))
                .Where(file => File.Exists(file))
                .FirstOrDefault();
        }

        private static FileStream WaitForFileStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare share = FileShare.ReadWrite, int sleepInterval = 1)
        {
            while (true)
            {
                try
                {
                    return new FileStream(path, fileMode, fileAccess, share);
                }
                catch (Exception)
                {
                    Thread.Sleep(sleepInterval);
                }
            }
        }
    }
}
