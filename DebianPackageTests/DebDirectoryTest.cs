using DebianPackage;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DebianPackageTests
{
    public class DebDirectoryTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void CanCreateValidateDelete()
        {
            var dir = new DebDirectory("NotAnExistingDir");

            Assert.IsFalse(dir.Validate().IsOk());
            dir.Create();
            Assert.IsTrue(dir.Validate().IsOk());
            dir.Delete();
            Assert.IsFalse(dir.Validate().IsOk());
        }
    }
}
