using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assignment4
{

    public class FileHandler
    {
        private readonly string filePath;
        private readonly int targetSize;
        private const int TaskCount = 25;
        private CancellationTokenSource cts;
        private readonly object fileLock = new object();

        public FileHandler(string filePath, int targetSize)
        {
            this.filePath = filePath;
            this.targetSize = targetSize;
            this.cts = new CancellationTokenSource();
        }

        public void RunTasks()
        {
            try
            {
                var monitorTask = Task.Run(() => MonitorFileSize(cts.Token), cts.Token);
                Parallel.For(0, TaskCount, new ParallelOptions { CancellationToken = cts.Token }, _ => WriteRandomData(cts.Token));
                monitorTask.Wait();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Tasks were cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred: " + ex.Message);
            }
            finally
            {
                // Print the final file size only once after all tasks have completed
                long finalSize;
                lock (fileLock)
                {
                    var fileInfo = new FileInfo(filePath);
                    finalSize = fileInfo.Length;
                }

                Console.WriteLine("All tasks have completed.");
                Console.WriteLine("Final file size: " + finalSize + " bytes");
                cts.Dispose();
            }
        }

        private void WriteRandomData(CancellationToken token)
        {
            var random = new Random();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var data = new string(Enumerable.Repeat(0, 36)
                        .Select(_ => (char)random.Next(32, 127))
                        .ToArray());

                    lock (fileLock)
                    {
                        File.AppendAllText(filePath, data);
                    }

                    Task.Delay(10).Wait();
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine("I/O error in writing: " + ioEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error in writing: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Write operation has ended.");
            }
        }

        private void MonitorFileSize(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Task.Delay(100).Wait();
                    long currentSize;

                    lock (fileLock)
                    {
                        var fileInfo = new FileInfo(filePath);
                        currentSize = fileInfo.Length;
                    }

                    Console.WriteLine("Current file size: " + currentSize + " bytes");

                    if (currentSize >= targetSize)
                    {
                        Console.WriteLine("Target size reached. Final size: " + currentSize + " bytes");
                        cts.Cancel();
                        break;
                    }
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine("I/O error in monitoring file size: " + ioEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error in monitoring file size: " + ex.Message);
            }
        }
    }
}