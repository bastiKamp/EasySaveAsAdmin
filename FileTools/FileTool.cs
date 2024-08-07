using System;
using System.IO;

namespace EasySaveAsAdmin.FileTools
{
    public static class FileTool
    {
        public static bool CanSave(string path)
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    return fileStream.CanWrite;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}