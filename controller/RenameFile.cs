using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using bulk_rename.model;

namespace bulk_rename.controller
{
    public class RenameFile
    {
        public const string RENAME_FILE_NAME = ".bulk_rename";
        private const int WAITTIME_FILEHANDLE = 100;

        public RenameFile(string directory)
        {
            this.directory = directory;

            FilePath = Path.Combine(directory, RENAME_FILE_NAME);

            watcher = new FileSystemWatcher(directory, RENAME_FILE_NAME);
            watcher.Changed += OnRenameFileChanged;
            watcher.Deleted += OnRenameFileDeleted;
        }

        private string directory;
        private FileSystemWatcher watcher;

        public string FilePath { get; }

        public event EventHandler<FileContentArgs> RenameFileChanged;
        public event EventHandler<EventArgs> RenameFileDeleted;

        public void Create(IEnumerable<(string, FileType)> initialFileNames)
        {
            if (!Directory.Exists(directory))
                throw new Exception("directory does not exist");

            using (StreamWriter writer = new StreamWriter(FilePath))
                foreach ((string file, _) in initialFileNames)
                    writer.WriteLine(file);

            File.SetAttributes(FilePath, File.GetAttributes(FilePath) | FileAttributes.Hidden);

            watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            watcher.EnableRaisingEvents = false;

            try
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
            }
            catch (IOException) {}
        }

        private void OnRenameFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name != RENAME_FILE_NAME) return;
            if (!File.Exists(e.FullPath)) return;

            WaitForFileHandle();
            FileStream renameFileStream = WaitForFileStream(e.FullPath, FileMode.Open, FileAccess.Read);

            List<string> currentFileNames = new List<string>();
            using (StreamReader reader = new StreamReader(renameFileStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    currentFileNames.Add(line);
            }

            RenameFileChanged?.Invoke(this, new FileContentArgs(currentFileNames));
        }

        private void OnRenameFileDeleted(object sender, FileSystemEventArgs e)
        {
            RenameFileDeleted?.Invoke(this, null);
        }

        private static void WaitForFileHandle()
        {
            Thread.Sleep(WAITTIME_FILEHANDLE);
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

        public class FileContentArgs : EventArgs
        {
            public FileContentArgs(IEnumerable<string> lines)
            {
                Lines = lines;
            }

            public IEnumerable<string> Lines { get; }
        }
    }
}
