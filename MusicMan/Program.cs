using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Console;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Corex.IO.Tools;

namespace MusicMan
{
    class Program
    {
        static string MusicDir = @"C:\Music";
        static void Main(string[] args)
        {
            MusicDir = ConfigurationManager.AppSettings["MusicDir"] ?? MusicDir;

            var helper = new ToolArgsInfo<ProgramOptions>();
            var options = helper.Parse(args);
            if (options.Help)
            {
                WriteLine(helper.GenerateHelp());
                return;
            }

            var file = options.InputFilename.ToFileInfo();
            var queries = file.Lines().Where(t=>t.IsNotNullOrEmpty()).ToList();
            var results = queries.Select(q=> new QueryResult { Query = q, AlbumDir = FindAlbumDir(q) }).ToList();
            var playlist = results.SelectMany(t => GetAudioFiles(t.AlbumDir)).ToList();
            if (options.Random)
            {
                playlist = Randomize(playlist);
            }
            playlist.ForEach(t=>WriteLine(t.FullName));
            File.WriteAllLines(file.FullName+".m3u", playlist.Select(t => t.FullName));
        }

        static Random Random = new Random();

        private static List<T> Randomize<T>(List<T> list)
        {
            var list2 = new List<T>();
            while (list.Count > 0)
            {
                var index = Random.Next(0, list.Count);
                list2.Add(list[index]);
                list.RemoveAt(index);
            }
            return list2;

        }

        static List<FileInfo> GetAudioFiles(DirectoryInfo dir)
        {
            return dir.GetFiles().Where(f => f.Extension == ".mp3" || f.Extension == ".flac").ToList();
        }

        private static DirectoryInfo FindAlbumDir(string q)
        {
            var dirs = MusicDir.ToDirectoryInfo().GetDirectories("*", SearchOption.AllDirectories);
            var list = dirs.Where(dir => DirMatchesQuery(dir, q)).ToList();
            if (list.Count == 0)
            {
                WriteLine("Can't find album for query: {0}", q);
                return null;
            }
            else if (list.Count > 1)
            {
                WriteLine("Multiple matches for query: {0}", q);
                list.ForEach(t => WriteLine(t));
            }
            return list[0];
        }
        static bool DirMatchesQuery(DirectoryInfo dir, string q)
        {
            var xxx = String.Format("{0} {1}", q, dir.FullName); ;
            //WriteLine("{0} {1}", q, dir.FullName);
            
            if (GetAudioFiles(dir).Count==0)
                return false;
            var s = dir.FullName.ToLower();
            q = q.ToLower();
            var tokens = q.Split(' ');
            if (tokens.All(token => s.Contains(token)))
                return true;
            return false;

        }
    }


    class ProgramOptions
    {
        [ToolArgCommand]
        public string InputFilename { get; set; }

        [ToolArgSwitch]
        public bool Random { get; set; }

        [ToolArgSwitch]
        public bool Help { get; set; }



    }

}
