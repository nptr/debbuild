using DebianPackage;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace DebianPackageTests
{
    public class DebBuilderTests
    {
        [SetUp]
        public void Setup()
        {
            var di = Directory.CreateDirectory("MinimalPackageTest");
            di.CreateSubdirectory("control");
            di.CreateSubdirectory("data");
        }

        [TearDown]
        public void TearDown()
        {
            var di = new DirectoryInfo("MinimalPackageTest");
            di.Delete(true);
        }

        [Test]
        public void CanCreateBasicArchive()
        {
            var attrib = new DebianPackage.FileAttributes()
            {
                UID = 1000,
                GID = 1001,
                UName = "user",
                GName = "user",
                FolderPermissions = Convert.ToInt32("775", 8),
                FilePermissions = Convert.ToInt32("664", 8),
            };
            
            using(var ms = new MemoryStream())
            {
                var builder = new DebBuilder("MinimalPackageTest", attrib);
                builder.CreateArchive(ms);

                byte[] deb = new byte[1024];

                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(deb, 0, 1024);

                // basic sanity check
                Assert.AreEqual(Encoding.ASCII.GetString(deb, 0, 8), "!<arch>\n");  // signature
                Assert.AreEqual(Encoding.ASCII.GetString(deb, 8, 16), "debian-binary/  "); // .deb arch identifier

                Assert.AreEqual(Encoding.ASCII.GetString(deb, 36, 6), "1000  ");    // uid
                Assert.AreEqual(Encoding.ASCII.GetString(deb, 42, 6), "1001  ");    // gid

                Assert.AreEqual(Encoding.ASCII.GetString(deb, 66, 2), "`\n");       // entry end
                Assert.AreEqual(Encoding.ASCII.GetString(deb, 68, 4), "2.0\n");     // version

                Assert.AreEqual(Encoding.ASCII.GetString(deb, 72, 16), "control.tar.gz/ "); // control archive
            }
        }
    }
}