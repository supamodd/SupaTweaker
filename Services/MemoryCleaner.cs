using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SupaTweaker.Services;

public static class MemoryCleaner
{
    public readonly record struct RamInfo(ulong Total, ulong Avail);

    public static RamInfo Read()
    {
        var s = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        GlobalMemoryStatusEx(ref s);
        return new RamInfo(s.TotalPhys, s.AvailPhys);
    }

    public static string Format(ulong bytes)
    {
        var gb = bytes / 1024d / 1024d / 1024d;
        return $"{gb:0.00} ГБ";
    }

    public static void Clean()
    {
        Enable("SeDebugPrivilege");
        Enable("SeIncreaseQuotaPrivilege");
        Enable("SeProfileSingleProcessPrivilege");

        EmptyProcessWorkingSets();
        FlushFileCache();
        MemoryList(2); // EmptyWorkingSets
        MemoryList(3); // FlushModifiedList
        MemoryList(4); // PurgeStandbyList
        MemoryList(5); // PurgeLowPriorityStandbyList
        GC.Collect();
        GC.WaitForPendingFinalizers();
        EmptyWorkingSet(Process.GetCurrentProcess().Handle);
    }

    private static void EmptyProcessWorkingSets()
    {
        foreach (var p in Process.GetProcesses())
        {
            try
            {
                EmptyWorkingSet(p.Handle);
                SetProcessWorkingSetSize(p.Handle, (nint)(-1), (nint)(-1));
            }
            catch { }
            finally { p.Dispose(); }
        }
    }

    private static void FlushFileCache()
    {
        SetSystemFileCacheSize((nint)(-1), (nint)(-1), 0);
    }

    private static void MemoryList(int command)
    {
        var ptr = Marshal.AllocHGlobal(4);
        try
        {
            Marshal.WriteInt32(ptr, command);
            NtSetSystemInformation(80, ptr, 4);
        }
        catch { }
        finally { Marshal.FreeHGlobal(ptr); }
    }

    private static void Enable(string name)
    {
        if (!OpenProcessToken(GetCurrentProcess(), 0x20 | 0x08, out var token)) return;
        try
        {
            if (!LookupPrivilegeValue(null, name, out var luid)) return;
            var tp = new TokenPrivileges { Count = 1, Luid = luid, Attr = 2 };
            AdjustTokenPrivileges(token, false, ref tp, 0, nint.Zero, nint.Zero);
        }
        finally { CloseHandle(token); }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TokenPrivileges
    {
        public int Count;
        public Luid Luid;
        public int Attr;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Luid
    {
        public uint Low;
        public int High;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx s);

    [DllImport("psapi.dll")]
    private static extern bool EmptyWorkingSet(nint h);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetProcessWorkingSetSize(nint h, nint min, nint max);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetSystemFileCacheSize(nint min, nint max, int flags);

    [DllImport("ntdll.dll")]
    private static extern int NtSetSystemInformation(int cls, nint info, int len);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(nint p, uint access, out nint token);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool LookupPrivilegeValue(string? sys, string name, out Luid luid);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AdjustTokenPrivileges(nint token, bool disable, ref TokenPrivileges neu, int len, nint old, nint ret);

    [DllImport("kernel32.dll")]
    private static extern nint GetCurrentProcess();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(nint h);
}
