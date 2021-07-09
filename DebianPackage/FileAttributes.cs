namespace DebianPackage
{
    public class FileAttributes
    {
        public FileAttributes()
        {
            UID = 0;
            GID = 0;
            UName = "root";
            GName = "root";
        }

        public int UID;
        public string UName;

        public int GID;
        public string GName;

        public int FilePermissions;
        public int FolderPermissions;
    }
}
