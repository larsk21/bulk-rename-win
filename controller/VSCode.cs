using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace bulk_rename.controller
{
    public static class VSCode
    {
        private const string VSCODE_EXECUTABLE = "code.cmd";
        private const string VSCODE_PROCESS_NAME = "Code";

        private const int WAITTIME_VSCODE_CLOSED = 60 * 1000;

        public static void OpenFile(string path)
        {
            string editorPath = FindExecutable(VSCODE_EXECUTABLE);

            ProcessStartInfo pci = new ProcessStartInfo()
            {
                FileName = editorPath,
                Arguments = "\"" + path + "\"",
                CreateNoWindow = true
            };
            Process.Start(pci);
        }

        public static void WaitForClose(Action callback)
        {
            Timer timer = null;
            timer = new Timer
            (
                state =>
                {
                    if (!IsRunning())
                    {
                        callback();
                        timer?.Dispose();
                    }
                },
                null,
                WAITTIME_VSCODE_CLOSED,
                WAITTIME_VSCODE_CLOSED
            );
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

        private static bool IsRunning()
        {
            return Process.GetProcessesByName(VSCODE_PROCESS_NAME).Any();
        }
    }
}
