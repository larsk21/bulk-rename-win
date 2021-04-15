using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using bulk_rename.model;

namespace bulk_rename.controller
{
    public static class Renamer
    {
        public static IEnumerable<(string, string, FileType)> GetRenameActions(
            string directory,
            IEnumerable<(string, FileType)> initialFileNames,
            IEnumerable<string> currentFileNames
        ) {
            if (initialFileNames.Count() != currentFileNames.Count())
                throw new Exception("deleted lines in rename file");

            foreach (
                (string initial, FileType type, string current) in
                initialFileNames.Zip(currentFileNames, (initial, current) => (initial.Item1, initial.Item2, current))
            ) {
                string initialPath = Path.Combine(directory, initial);
                string currentPath = Path.Combine(directory, current);

                if (initial == current)
                    {}
                else if (string.IsNullOrWhiteSpace(current))
                    yield return (initialPath, null, type);
                else
                    yield return (initialPath, currentPath, type);
            }
        }

        public static void Rename(string from, string to, FileType type)
        {
            if (to == null)
            {
                if (type == FileType.File)
                    File.Delete(from);
                else if (type == FileType.Directory)
                    Directory.Delete(from);
            }
            else
            {
                if (type == FileType.File)
                    File.Move(from, to);
                else if (type == FileType.Directory)
                    Directory.Move(from, to);
            }
        }
    }
}
