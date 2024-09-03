using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnrealSharpBuildTool.Models
{
    public static class FileSystemInfoExtensions
    {
        public static void ThrowIfNotExists(this FileSystemInfo info, [CallerArgumentExpression(nameof(info))] string name = default!)
        {
            if (info is DirectoryInfo)
            {
                throw new DirectoryNotFoundException($"member: {name}, path: {info.FullName}");
            }
            else if (info is FileInfo)
            {
                throw new FileNotFoundException($"member: {name}, path: {info.FullName}");
            }
            else
            {
                throw new Exception($"Unknown file system info type; member: {name}, path: {info.FullName}");
            }
        }

        public static T ReturnIfExists<T>(this T info, [CallerArgumentExpression(nameof(info))] string name = default!)
            where T : FileSystemInfo
        {
            if (info.Exists)
            {
                return info;
            }
            else
            {
                if (info is DirectoryInfo)
                {
                    throw new DirectoryNotFoundException($"member: {name}, path: {info.FullName}");
                }
                else if (info is FileInfo)
                {
                    throw new FileNotFoundException($"member: {name}, path: {info.FullName}");
                }
                else
                {
                    throw new Exception($"Unknown file system info type; member: {name}, path: {info.FullName}");
                }
            }
        }

        public static void CreateIfNotExists(this DirectoryInfo directory)
        {
            if (!directory.Exists)
            {
                directory.Create();
            }
        }

        public static void DeleteIfExists(this FileSystemInfo info)
        {
            if (info.Exists)
            {
                info.Delete();
            }
        }
    }
}
