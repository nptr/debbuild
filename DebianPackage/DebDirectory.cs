using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DebianPackage
{
    public class DebDirectory
    {
        private const string CONTROL_FOLDER = "control";
        private const string DATA_FOLDER = "data";

        private string m_directory;

        public DebDirectory(string path)
        {
            m_directory = path;
        }

        public void Create()
        {
            var di = new DirectoryInfo(m_directory);
            di.Create();
            di.CreateSubdirectory(CONTROL_FOLDER);
            di.CreateSubdirectory(DATA_FOLDER);
        }

        public void Delete()
        {
            var di = new DirectoryInfo(m_directory);
            di.Delete(true);
        }

        public Result Validate()
        {
            if (!Directory.Exists(m_directory))
            {
                return Result.Fail("Package folder does not exist or is not accessible!");
            }

            string controlFolder = Path.Combine(m_directory, CONTROL_FOLDER);
            if (!Directory.Exists(controlFolder))
            {
                return Result.Fail("Package folder does not contain a \"control\" folder!");
            }

            string dataFolder = Path.Combine(m_directory, DATA_FOLDER);
            if (!Directory.Exists(dataFolder))
            {
                return Result.Fail("Package folder does not contain a \"data\" folder!");
            }

            return Result.Ok();
        }
    }
}
