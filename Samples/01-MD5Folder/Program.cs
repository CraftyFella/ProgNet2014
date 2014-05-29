using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ActorsLifeForMe.MD5Folder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ready to start.");
            Console.ReadLine();

            ProcessFolder(@"D:\Movies\");

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

            var filePaths = new BufferBlock<string>();


            var display = new ActionBlock<Tuple<string, string>>(new Action<Tuple<string, string>>(DisplayMD5FromFileOnConsole));

            var createBlocks = new List<TransformBlock<string, Tuple<string, string>>>();

            for (int i = 0; i < 4; i++)
            {
                var createMD5 = new TransformBlock<string, Tuple<string, string>>(
                    filename => MD5WithFileName(filename), blockConfiguration);




                createMD5.LinkTo(display, new DataflowLinkOptions {PropagateCompletion = true});
                filePaths.LinkTo(createMD5, new DataflowLinkOptions {PropagateCompletion = true});
                createBlocks.Add(createMD5);
            }

            var files = System.IO.Directory.GetFiles(folder);
            foreach (var filepath in files)
            {
                filePaths.Post(filepath);
            }

            filePaths.Complete();
            display.Completion.Wait();
        }

        private static Tuple<string, string> MD5WithFileName(string filename)
        {
            Console.WriteLine("Begin : {0}", Path.GetFileName(filename));

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
