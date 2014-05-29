using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;

namespace ActorsLifeForMe.MD5Folder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ready to start.");
            Console.ReadLine();

            ProcessFolder(@"D:\Movies");

            Console.WriteLine("Completed.");
            Console.ReadLine();
        }

        private static void ProcessFolder(string folder)
        {
            // this quickly modifies our processing to be multi-threaded
            var blockConfiguration = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 1,         // One thread
                BoundedCapacity = 1                 // One item in the queue at any one time.
            };


            var findContentsOfFolder = new TransformManyBlock<string, string>(f => Directory.GetFileSystemEntries(f));
            var display = new ActionBlock<Tuple<string, string>>(new Action<Tuple<string, string>>(DisplayMD5FromFileOnConsole));
            var createBlocks = new List<TransformBlock<string, Tuple<string, string>>>();
            findContentsOfFolder.LinkTo(findContentsOfFolder, Directory.Exists);        // If it's a directory link back to it's self

            for (var i = 0; i < 4; i++)
            {
                var createMD5 = new TransformBlock<string, Tuple<string, string>>(
                    filename => MD5WithFileName(filename), blockConfiguration);




                createMD5.LinkTo(display, new DataflowLinkOptions {PropagateCompletion = true});
                findContentsOfFolder.LinkTo(createMD5, File.Exists);                        // Only pass on if it's a file.
                createBlocks.Add(createMD5);
            }

           

            findContentsOfFolder.Post(folder);

            //filePaths.Complete();
            display.Completion.Wait();
        }

        private static Tuple<string, string> MD5WithFileName(string filename)
        {
            Console.WriteLine("Begin : {0}", Path.GetFileName(filename));
            Console.WriteLine("and in : {0}", Path.GetDirectoryName(filename));

            return Tuple.Create(filename, MD5FromFile(filename));
        }

        private static void DisplayMD5FromFileOnConsole(Tuple<string, string> filepath)
        {
            Console.WriteLine("End : {0} : {1}", Path.GetFileName(filepath.Item1), filepath.Item2);
        }

        private static string MD5FromFile(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
    }
}
