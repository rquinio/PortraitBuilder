using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace PortraitBuilder.Model
{
    public enum OS
    {
        Windows,
        Mac,
        Linux,
        Other
    }

    public class OSUtils
    {
        public static OS determineOS() {
            if (Path.DirectorySeparatorChar == '\\')
                return OS.Windows;
            else if (IsRunningOnMac())
                return OS.Mac;
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
                return OS.Linux;
            else
                return OS.Other;
        }

        //From Managed.Windows.Forms/XplatUI
        [DllImport("libc")]
        static extern int uname(IntPtr buf);

        static bool IsRunningOnMac() {
            IntPtr buf = IntPtr.Zero;
            try {
                buf = Marshal.AllocHGlobal(8192);
                // This is a hacktastic way of getting sysname from uname ()
                if (uname(buf) == 0) {
                    string os = Marshal.PtrToStringAnsi(buf);
                    if (os == "Darwin")
                        return true;
                }
            } catch {
            } finally {
                if (buf != IntPtr.Zero)
                    Marshal.FreeHGlobal(buf);
            }
            return false;
        }
    }
}
