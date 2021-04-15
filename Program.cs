using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using bulk_rename.controller;
using bulk_rename.model;

namespace bulk_rename
{
    public class Program
    {
        public Program(string directory)
        {
            this.directory = directory;

            finishedEvent = new CountdownEvent(1);
        }

        private string directory;
        private IEnumerable<(string, FileType)> initialFileNames;

        private CountdownEvent finishedEvent;

        public void Execute()
        {
            // create a list to force evaluation of the enumerable at this point
            initialFileNames = new List<(string, FileType)>(FileSystem.GetFilesAndDirectories(directory));

            RenameFile renameFile = new RenameFile(directory);
            renameFile.RenameFileChanged += OnRenameFileChanged;
            renameFile.RenameFileDeleted += OnRenameFileDeleted;
            renameFile.Create(initialFileNames);

            VSCode.OpenFile(renameFile.FilePath);
            VSCode.WaitForClose(OnVSCodeClosed);
            
            finishedEvent.Wait();
            renameFile.Dispose();
        }

        private void OnRenameFileChanged(object sender, RenameFile.FileContentArgs e)
        {
            IEnumerable<(string, string, FileType)> renameActions = Renamer.GetRenameActions(directory, initialFileNames, e.Lines);

            foreach ((string from, string to, FileType type) in renameActions)
                Renamer.Rename(from, to, type);

            finishedEvent.Signal();
        }

        private void OnRenameFileDeleted(object sender, EventArgs e)
        {
            finishedEvent.Signal();
        }

        private void OnVSCodeClosed()
        {
            finishedEvent.Signal();
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length < 1) return;

            string directory = args[0];
            if (!Directory.Exists(directory)) return;

            new Program(directory).Execute();
        }
    }
}
