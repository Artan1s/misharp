using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using ConsoleApplication1.CSharp;

namespace Watcher
{
    public class Program
    {
        private static FileSystemWatcher watcher;
        private static string[] args;

        public static void Main()
        {
            Run();

        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {
            args = System.Environment.GetCommandLineArgs();

            // If a directory is not specified, exit program. 
            if (args.Length != 3)
            {
                // Display the proper way to call the program.
                Console.WriteLine("Usage: Watcher.exe (input_directory) (output_java_directory)");
                return;
            }

            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = args[1];
            watcher.IncludeSubdirectories = true;
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = "*.cs";

            // Add event handlers.
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnChanged;

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            Console.WriteLine("Press \'q\' to quit the watcher.");
            while (Console.Read() != 'q')
            {
            }
        }

        // Define the event handlers. 
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            try
            {
                watcher.EnableRaisingEvents = false;

                Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);

                (new JavaGenerator()).Generate(args[1], args[2]);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("OK. ");
                Console.ResetColor();
                Console.WriteLine("Java code generated successfully");
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error. ");
                Console.ResetColor();
                Console.WriteLine("Invalid input sources!!!" + exception.Message);
            }
            finally
            {
                watcher.EnableRaisingEvents = true;
            }
        }
    }
}
