using System;
using System.IO;

namespace SaveAsAdmin
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                return;
            }
            
            var filePath = args[0];
            if (!IsInputValid(filePath)) return;
            
            var content = args[1];
            var decodedContent = DecodeContent(content);
            
            File.WriteAllText(filePath, decodedContent);
        }

        private static string DecodeContent(string encodedContent)
        {
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(encodedContent);
                var decodedContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

                return decodedContent;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool IsInputValid(string filePath)
        {
            if(string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("Please provide a file path and content to save.");
                return false;
            }
            
            if(!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist.");
                return false;
            }
            
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var canWrite = fileStream.CanWrite;
                if (!canWrite)
                {
                    return false;
                }
            }

            return true;
        }
    }
}