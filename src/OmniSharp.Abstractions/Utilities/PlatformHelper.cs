using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OmniSharp.Utilities
{
    public static class PlatformHelper
    {
        private static Lazy<bool> _isMono = new Lazy<bool>(() => Type.GetType("Mono.Runtime") != null);
        private static Lazy<string> _monoPath = new Lazy<string>(FindMonoPath);
        private static Lazy<string> _monoXBuildFrameworksDirPath = new Lazy<string>(FindMonoXBuildFrameworksDirPath);

        public static bool IsMono => _isMono.Value;
        public static string MonoFilePath => _monoPath.Value;
        public static string MonoXBuildFrameworksDirPath => _monoXBuildFrameworksDirPath.Value;

        public static bool IsWindows => Path.DirectorySeparatorChar == '\\';

        // CharSet.Ansi is UTF8 on Unix
        [DllImport("libc", EntryPoint = "realpath", CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Unix_realpath(string path, IntPtr buffer);

        [DllImport("libc", EntryPoint = "free", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Unix_free(IntPtr ptr);

        private static string RealPath(string path)
        {
            if (IsWindows)
            {
                throw new PlatformNotSupportedException($"{nameof(RealPath)} can only be called on Unix.");
            }

            var ptr = Unix_realpath(path, IntPtr.Zero);
            var result = Marshal.PtrToStringAnsi(ptr); // uses UTF8 on Unix
            Unix_free(ptr);

            return result;
        }

        private static string FindMonoPath()
        {
            if (IsWindows)
            {
                throw new PlatformNotSupportedException($"{nameof(FindMonoPath)} can only be called on Unix.");
            }

            return RealPath("mono");
        }

        private static string FindMonoXBuildFrameworksDirPath()
        {
            const string defaultXBuildFrameworksDirPath = "/usr/lib/mono/xbuild-frameworks";
            if (Directory.Exists(defaultXBuildFrameworksDirPath))
            {
                return defaultXBuildFrameworksDirPath;
            }

            // The normal Unix path doesn't exist, so we'll fallback to finding Mono using the
            // runtime location. This is the likely situation on macOS.
            var monoFilePath = MonoFilePath;
            if (string.IsNullOrEmpty(monoFilePath))
            {
                return null;
            }

            // mono should be located within a directory that is a sibling to the lib directory.
            var monoDirPath = Path.GetDirectoryName(monoFilePath);

            // The base directory is one folder up
            var monoBaseDirPath = Path.Combine(monoDirPath, "..");
            monoBaseDirPath = Path.GetFullPath(monoBaseDirPath);

            // We expect the xbuild-frameworks to be in /Versions/Current/lib/mono/xbuild-frameworks.
            var monoXBuildFrameworksDirPath = Path.Combine(monoBaseDirPath, "lib/mono/xbuild-frameworks");
            monoXBuildFrameworksDirPath = Path.GetFullPath(monoXBuildFrameworksDirPath);

            return Directory.Exists(monoXBuildFrameworksDirPath)
                ? monoXBuildFrameworksDirPath
                : null;
        }
    }
}
