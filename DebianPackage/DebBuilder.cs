using System;
using System.IO;
using System.Text;

namespace DebianPackage
{
    public class PermissionEntry
    {
        public string Path;
        public int Permission;
    }

    public class DebBuilder
    {
        private const string ARCHIVE_SIG = "!<arch>\n";
        private const string ENTRY_END = "`\n";
        
        private const int FILE_IDENT_SIZE = 16;
        private const int FILE_TIME_SIZE = 12;
        private const int FILE_MODE_SIZE = 8;
        private const int FILE_SIZE_SIZE = 10;
        private const int OWNER_ID_SIZE = 6;
        private const int GROUP_ID_SIZE = 6;

        private const string FILE_MODE_644 = "100644";

        private const string DEBIAN_BINARY_FILE = "debian-binary";
        private const string DEBIAN_BINARY_CONTENT = "2.0\n";

        private const string CONTROL_FOLDER = "control";
        private const string CONTROL_ARCHIVE = "control.tar.gz";

        private const string DATA_FOLDER = "data";
        private const string DATA_ARCHIVE = "data.tar.gz";

        private string m_packageFolder;
        private FileAttributes m_attributes;

        public DebBuilder(string packageFolder, FileAttributes attrib)
        {
            m_packageFolder = packageFolder;
            m_attributes = attrib;
        }

        private uint ToUnixTimestamp(DateTime dt)
        {
            return (uint)(dt.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private void WriteDebEntry(Stream outStream, Stream inStream, string fileIdentifier, DateTime fileModTime, FileAttributes attrib)
        {
            /* A .deb file is actually an archive produced by the "ar" Unix utility.
             * Installing it on Windows is tedious and overkill (via cygwin or gcc),
             * hence this tool.
             * 
             * Anyway, the format is a semi-standard best read on Wikipedia:
             * https://en.wikipedia.org/wiki/Ar_(Unix)
             */

            /*
             * System V ar uses a '/' character (0x2F) to mark the end of the filename; 
             * this allows for the use of spaces without the use of an extended filename.
             */
            fileIdentifier = (fileIdentifier + '/').PadRight(FILE_IDENT_SIZE);
            outStream.Write(Encoding.ASCII.GetBytes(fileIdentifier), 0, FILE_IDENT_SIZE);

            string time = ToUnixTimestamp(fileModTime).ToString().PadRight(FILE_TIME_SIZE);
            outStream.Write(Encoding.ASCII.GetBytes(time), 0, FILE_TIME_SIZE);

            string owner = attrib.UID.ToString().PadRight(OWNER_ID_SIZE);
            outStream.Write(Encoding.ASCII.GetBytes(owner), 0, OWNER_ID_SIZE);

            string group = attrib.GID.ToString().PadRight(GROUP_ID_SIZE);
            outStream.Write(Encoding.ASCII.GetBytes(group), 0, GROUP_ID_SIZE);

            string mode = FILE_MODE_644.PadRight(FILE_MODE_SIZE);
            outStream.Write(Encoding.ASCII.GetBytes(mode), 0, FILE_MODE_SIZE);

            string size = inStream.Length.ToString().PadRight(FILE_SIZE_SIZE);
            outStream.Write(Encoding.ASCII.GetBytes(size), 0, FILE_SIZE_SIZE);
            
            outStream.Write(Encoding.ASCII.GetBytes(ENTRY_END), 0, ENTRY_END.Length);

            byte[] buffer = new byte[8192];
            int read;
            while ((read = inStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                outStream.Write(buffer, 0, read);
            }

            /*
             * Each data section is 2 byte aligned. If it would end on an
             * odd offset, a newline ('\n', 0x0A) is used as filler.
             */
            if (outStream.Position % 2 != 0)
            {
                outStream.WriteByte(0x0A);
            }

            return;
        }

        public void CreateArchive(Stream outStream)
        {
            byte[] sig = Encoding.ASCII.GetBytes(ARCHIVE_SIG);
            outStream.Write(sig, 0, sig.Length);

            // write debian-binary
            using (var ms = new MemoryStream(Encoding.ASCII.GetBytes(DEBIAN_BINARY_CONTENT)))
            {
                WriteDebEntry(outStream, ms, DEBIAN_BINARY_FILE, DateTime.UtcNow, m_attributes);
            }

            // write control.tar.gz
            string controlFolder = Path.Combine(m_packageFolder, CONTROL_FOLDER);
            using (var ms = new MemoryStream())
            {
                TarUtil.ArchiveFromFolder(ms, controlFolder, m_attributes);
                ms.Seek(0, SeekOrigin.Begin);
                WriteDebEntry(outStream, ms, CONTROL_ARCHIVE, DateTime.UtcNow, m_attributes);
            }

            // write data.tar.gz
            string dataFolder = Path.Combine(m_packageFolder, DATA_FOLDER);
            using (var ms = new MemoryStream())
            {
                TarUtil.ArchiveFromFolder(ms, dataFolder, m_attributes);
                ms.Seek(0, SeekOrigin.Begin);
                WriteDebEntry(outStream, ms, DATA_ARCHIVE, DateTime.UtcNow, m_attributes);
            }
        }

        public void CreateArchive(string outFile)
        {
            using(var fs = File.Open(outFile, FileMode.Create))
            {
                CreateArchive(fs);
            }
        }
    }
}
