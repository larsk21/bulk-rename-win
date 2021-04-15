using System.Collections.Generic;
using System.IO;
using System.Linq;
using bulk_rename.model;

namespace bulk_rename.controller
{
    public static class FileSystem
    {
        public static IEnumerable<(string, FileType)> GetFilesAndDirectories(string directory)
        {
            return Directory
                .EnumerateDirectories(directory)
                .Select(item => (Path.GetFileName(item) + Path.DirectorySeparatorChar, FileType.Directory))
                .Concat(
                    Directory
                    .EnumerateFiles(directory)
                    .Select(item => (Path.GetFileName(item), FileType.File))
                );
        }
    }
}
