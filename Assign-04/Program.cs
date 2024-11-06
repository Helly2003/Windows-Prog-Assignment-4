/*
FILE          : prgram.cs
PROJECT       : Assignment 4
PROGRAMMER    : Helly Shah (8958841)
FIRST VERSION : November 5, 2024
DESCRIPTION   : 
*/

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assignment4
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2 || args[0] == "/?")
                {
                    DisplayUsage();
                    return;
                }

                string filePath = args[0];
                if (!int.TryParse(args[1], out int targetSize) || targetSize < 1000 || targetSize > 20000000)
                {
                    Console.WriteLine("Error: Invalid file size. Must be between 1,000 and 20,000,000 characters.");
                    DisplayUsage();
                    return;
                }

                if (File.Exists(filePath))
                {
                    Console.Write("File '" + filePath + "' already exists. Overwrite? (y/n): ");
                    if (Console.ReadLine().ToLower() != "y")
                    {
                        Console.WriteLine("Operation cancelled.");
                        return;
                    }
                    File.Delete(filePath);
                }

                var fileHandler = new FileHandler(filePath, targetSize);
                fileHandler.RunTasks();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Program has ended. Press any key to exit.");
                Console.ReadKey();
            }
        }

        static void DisplayUsage()
        {
            Console.WriteLine("Usage: program.exe <filePath> <targetSize>");
            Console.WriteLine("  filePath: Path to the file to be written");
            Console.WriteLine("  targetSize: Target file size (1,000 - 20,000,000 characters)");
        }
    }
}

