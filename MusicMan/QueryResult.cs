using System;
using System.IO;

namespace MusicMan
{
    internal class QueryResult
    {
        public DirectoryInfo AlbumDir { get; internal set; }
        public string Query { get; internal set; }
    }
}