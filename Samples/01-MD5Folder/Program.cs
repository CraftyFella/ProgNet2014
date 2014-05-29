using System;
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
            var makeMD5AndDisplayToConsole = new ActionBlock<string>(new Action<string>(GetMD5FromFileAndDisplay));

            var files = Directory.GetFiles(folder);
            foreach (var filepath in files)
            {
                makeMD5AndDisplayToConsole.Post(filepath);
            }

            makeMD5AndDisplayToConsole.Complete();              // Done
            makeMD5AndDisplayToConsole.Completion.Wait();       // Block until we're done

        }

        private static void GetMD5FromFileAndDisplay(string filepath)
        {
            Console.WriteLine("Begin {0} : ", Path.GetFileName(filepath));
            Console.WriteLine("End {0} : {1}", Path.GetFileName(filepath), MD5FromFile(filepath));
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

        private static void WriteMD5ForFile(string filename)
        {
            var md5Hash = MD5FromFile(filename);

            System.IO.File.WriteAllText(filename + ".tpl.md5", md5Hash);
        }
    }
}
