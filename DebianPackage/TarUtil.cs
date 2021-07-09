using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;
using System.Text;

namespace DebianPackage
{
    class TarUtil
    {
        /// <summary>
        /// Make a path .tar compatible by making it relative and enforcing forward slash.
        /// Basically a Unix path in the form of "./dir1/dir2/file".
        /// </summary>
        /// <param name="path">The absolute input path.</param>
        /// <param name="relativeTo">The path to which the input path shall be relative to.</param>
        /// <returns></returns>
        private static string PathMakeTarCompatible(string path, string relativeTo)
        {
            var s = new StringBuilder(path);
            s.Replace(relativeTo, "");
            s.Replace("\\", "/");

            if (s[0] != '/')
                s.Insert(0, "./");
            else s.Insert(0, ".");

            return s.ToString();
        }

        private static string PathEnsureTrailingSlash(string path)
        {
            if (!path.EndsWith("\\") && !path.EndsWith("/"))
            {
                path += "/";
            }

            return path;
        }

        private static void AddFolderRecursive(TarArchive archive, string root, string folder, FileAttributes attrib)
        {
            var files = Directory.EnumerateFileSystemEntries(folder);
            foreach (string file in files)
            {
                string tarName = PathMakeTarCompatible(file, root);

                var dir = new DirectoryInfo(file);
                if (!dir.Exists)
                {
                    var entry = TarEntry.CreateEntryFromFile(file);
                    {
                        entry.Name = tarName;
                        entry.TarHeader.Mode = attrib.FilePermissions;
                    }
                    archive.WriteEntry(entry, false);
                }
                else
                {
                    var entry = new TarEntry(new TarHeader()
                    {
                        Name = tarName,
                        TypeFlag = TarHeader.LF_DIR,
                        Mode = attrib.FolderPermissions,
                        ModTime = dir.LastWriteTimeUtc,
                        Size = 0
                    });
                    archive.WriteEntry(entry, false);

                    AddFolderRecursive(archive, root, file, attrib);
                }
            }
        }

        internal static void ArchiveFromFolder(Stream outStream, string folder, FileAttributes attrib)
        {
            folder = PathEnsureTrailingSlash(folder);

            using (var gzStream = new GZipOutputStream(outStream))
            using (var archive = TarArchive.CreateOutputTarArchive(gzStream))
            {
                gzStream.IsStreamOwner = false;

                archive.SetUserInfo(
                    attrib.UID,
                    attrib.UName,
                    attrib.GID,
                    attrib.GName);

                TarEntry tarRoot = new TarEntry(new TarHeader()
                {
                    Name = "./",
                    TypeFlag = TarHeader.LF_DIR,
                    Mode = attrib.FolderPermissions,
                    ModTime = DateTime.UtcNow,
                    Size = 0
                });
                archive.WriteEntry(tarRoot, false);

                AddFolderRecursive(archive, folder, folder, attrib);

                archive.Close();
            }
        }
    }
}
