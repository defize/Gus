namespace Defize.Gus
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Authentication;
    using System.Text;

    internal class FileSystemHelper
    {
        private readonly GusTaskExecutionContext _context;

        public FileSystemHelper(GusTaskExecutionContext context)
        {
            _context = context;
        }

        public ICollection<FileInfo> GetScriptFiles(string path)
        {
            if (File.Exists(path))
            {
                return new List<FileInfo> {new FileInfo(path)};
            }

            if (Directory.Exists(path))
            {
                var directory = new DirectoryInfo(path);
                var scriptFiles = directory.GetFiles("*.sql", SearchOption.AllDirectories);

                var duplicates = FindDuplicates(scriptFiles);
                if (duplicates.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("The source '{0}' contains one or more files with the same name which is not supported. The following file names are duplicated:", path);
                    foreach (var duplicate in duplicates)
                    {
                        sb.AppendLine(duplicate);
                    }

                    _context.RaiseExecutionEvent(ExecutionEventType.Error, sb.ToString());
                    return null;
                }

                return scriptFiles;
            }

            _context.RaiseExecutionEvent(ExecutionEventType.Error, string.Format("The path '{0}' could not be found.", path));
            return null;
        }

        public string GetFileHash(FileInfo file)
        {
            using (var fs = file.OpenRead())
            {
                return fs.ComputeHashCode(HashAlgorithmType.SHA512);
            }
        }

        private static ICollection<string> FindDuplicates(IEnumerable<FileInfo> files)
        {
            var hashset = new HashSet<string>();
            var duplicates = new HashSet<string>();

            foreach (var file in files)
            {
                if (!hashset.Add(file.Name.ToUpper()))
                {
                    duplicates.Add(file.Name);
                }
            }

            return duplicates.ToArray();
        }
    }
}
