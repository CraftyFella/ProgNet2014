using System;
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
                MaxDegreeOfParallelism = 4
            };

            var createMD5 = new TransformBlock<string, Tuple<string, string>>(
                filename => MD5WithFileName(filename), blockConfiguration);


            var display = new ActionBlock<Tuple<string, string>>(new Action<Tuple<string, string>>(DisplayMD5FromFileOnConsole));

            createMD5.LinkTo(display, new DataflowLinkOptions { PropagateCompletion = true });

            var files = System.IO.Directory.GetFiles(folder);
            foreach (var filepath in files)
            {
                createMD5.Post(filepath);
            }

            createMD5.Complete();
            display.Completion.Wait();
        }

        private static Tuple<string, string> MD5WithFileName(string filename)
        {
            return Tuple.Create(filename, MD5FromFile(filename));
        }

        private static void DisplayMD5FromFileOnConsole(Tuple<string, string> filepath)
        {
            Console.WriteLine("Begin : {0}", Path.GetFileName(filepath.Item1));
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
