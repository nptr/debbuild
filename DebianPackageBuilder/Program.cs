using DebianPackage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace DebianPackageBuilder
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return 1;
            }

            if (args.Length <= 3)
            {
                Console.WriteLine("Missing arguments! Not all mandatory arguments were supplied.");
                return 1;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("The specified folder does not exist!");
                return 1;
            }
            string folder = args[0];
            string outfile = args[1];
            
            int uid;
            if (!Int32.TryParse(args[2], out uid))
            {
                Console.WriteLine("Argument <uid> is not an integer!");
                return 1;
            }
            int gid = uid;
            
            string uname = args[3];
            string gname = args[3];

            if (args.Length > 4)
            {
                if (!Int32.TryParse(args[4], out gid))
                {
                    Console.WriteLine("Argument [gid] is not an integer!");
                    return 1;
                }
            }

            if (args.Length > 5)
            {
                gname = args[5];
            }

            if (!DoWork(folder, outfile, uid, uname, gid, gname))
            {
                return 1;
            }

            Console.WriteLine("Successfully created \"" + outfile + "\".");
            return 0;
        }

        static void PrintHelp()
        {
            Console.WriteLine("debbuild Utility Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (C) Jakob K.");
            Console.WriteLine("");
            Console.WriteLine("Synopsis: debbuild <folder> <outfile> <uid> <uname> [gid] [gname]");
            Console.WriteLine("");
            Console.WriteLine("Options:");
            Console.WriteLine("    folder    The folder containing the control and data folders.");
            Console.WriteLine("    outfile   The name and path for the resulting package.");
            Console.WriteLine("    uid       The owning user id for the contained files.");
            Console.WriteLine("              If not otherwise specified, also the group id.");
            Console.WriteLine("    uname     The owning users name.");
            Console.WriteLine("              If not otherwise specified, also the group name.");
            Console.WriteLine("    gid       Specifies the group id.");
            Console.WriteLine("    gname     Specifies the group name.");
        }

        static bool DoWork(string folder, string outfile,
            int uid, string uname,
            int gid, string gname)
        {
            try
            {
                using (var fs = File.Open(outfile, FileMode.Create))
                {
                    var attrib = new DebianPackage.FileAttributes()
                    {
                        UID = uid,
                        GID = gid,
                        UName = uname,
                        GName = gname,
                        FolderPermissions = Convert.ToInt32("755", 8),
                        FilePermissions = Convert.ToInt32("644", 8),
                    };

                    var deb = new DebBuilder(folder, attrib);
                    deb.CreateArchive(fs);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
