using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Console;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MusicMan
{
    class Program
    {
        static string MusicDir = @"C:\Music";
        static void Main(string[] args)
        {
            MusicDir = ConfigurationManager.AppSettings["MusicDir"] ?? MusicDir;
            var file = args[0].ToFileInfo();
            var queries = file.Lines().Where(t=>t.IsNotNullOrEmpty()).ToList();
            var results = queries.Select(q=> new QueryResult { Query = q, AlbumDir = FindAlbumDir(q) }).ToList();
            var playlist = results.SelectMany(t => GetAudioFiles(t.AlbumDir)).ToList();
            playlist.ForEach(t=>WriteLine(t.FullName));
            File.WriteAllLines(file.FullName+".m3u", playlist.Select(t => t.FullName));
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

}
