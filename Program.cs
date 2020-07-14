using System;
using System.IO;

namespace bulk_rename
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length < 1) return;

            string directory = args[0];
            if (!Directory.Exists(directory)) return;

            BulkRenamer renamer = new BulkRenamer(directory);
            renamer.Rename();
            renamer.Wait();
        }
    }
}
