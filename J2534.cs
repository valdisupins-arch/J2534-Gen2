using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace DensoGen2
{
    public sealed class J2534DeviceInfo
    {
        public string Name, Vendor, Library;
        public int? Bits;
        public override string ToString()
        {
            return (Name ?? "") + "  [" + (Vendor ?? "") + " " + (Bits.HasValue ? Bits + "b" : "?") + "]  " + Path.GetFileName(Library ?? "");
        }

        public static List<J2534DeviceInfo> ListDevices()
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<J2534DeviceInfo>();
            Read(RegistryHive.LocalMachine, @"SOFTWARE\PassThruSupport.04.04", seen, list);
            Read(RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\PassThruSupport.04.04", seen, list);
            list.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase));
            return list;
        }

        static void Read(RegistryHive hive, string path, HashSet<string> seen, List<J2534DeviceInfo> list)
        {
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry32))
                using (var key = baseKey.OpenSubKey(path))
                {
                    if (key == null) return;
                    foreach (var sub in key.GetSubKeyNames())
                    using (var sk = key.OpenSubKey(sub))
                    {
                        if (sk == null) continue;
                        string lib = sk.GetValue("FunctionLibrary") as string;
                        if (string.IsNullOrEmpty(lib) || !seen.Add(lib)) continue;
                        list.Add(new J2534DeviceInfo
                        {
                            Name = (sk.GetValue("Name") as string) ?? sub,
                            Vendor = sk.GetValue("Vendor") as string ?? "",
                            Library = lib,
                            Bits = PeBits(lib)
                        });
                    }
                }
            }
            catch { }
        }

        static int? PeBits(string path)
        {
            try
            {
                using (var fs = File.OpenRead(path))
                using (var br = new BinaryReader(fs))
                {
                    if (br.ReadUInt16() != 0x5A4D) return null;
                    fs.Seek(0x3C, SeekOrigin.Begin);
                    int pe = br.ReadInt32();
                    fs.Seek(pe, SeekOrigin.Begin);
                    if (br.ReadUInt32() != 0x4550) return null;
                    ushort m = br.ReadUInt16();
                    if (m == 0x14C) return 32;
                    if (m == 0x8664) return 64;
                }
            }
            catch { }
            return null;
        }
    }

    public sealed class J2534 : IDisposable
    {
        const uint ISO15765 = 6;
        const uint CAN = 5;
        const uint FLOW = 3;
        const uint SET_CONFIG = 2;
        const uint CLEAR_TX = 7;
        const uint CLEAR_RX = 8;
        const uint CLEAR_FILTERS = 10;
        const uint PAD = 0x40;
        const uint FUNC = 1u << 28;

        IntPtr _dll;
        uint _dev, _ch;
        bool _open, _conn;
        PtOpen _openFn;
        PtClose _closeFn;
        PtConnect _connectFn;
        PtDisconnect _discFn;
        PtRead _readFn;
        PtWrite _writeFn;
        PtFilter _filterFn;
        PtIoctl _ioctlFn;
        PtErr _errFn;
        PtStartPer _startPer;
        PtStopPer _stopPer;
        uint _perId, _perId2;
        bool _perOn, _per2On;
        uint _canCh;
        bool _canConn;

        delegate int PtOpen(IntPtr name, ref uint id);
        delegate int PtClose(uint id);
        delegate int PtConnect(uint id, uint proto, uint flags, uint baud, ref uint ch);
        delegate int PtDisconnect(uint ch);
        delegate int PtRead(uint ch, IntPtr msg, ref uint n, uint timeout);
        delegate int PtWrite(uint ch, IntPtr msg, ref uint n, uint timeout);
        delegate int PtFilter(uint ch, uint type, IntPtr mask, IntPtr pat, IntPtr flow, ref uint fid);
        delegate int PtIoctl(uint ch, uint ioctl, IntPtr inp, IntPtr outp);
        delegate int PtErr(StringBuilder s);
        delegate int PtStartPer(uint ch, IntPtr msg, ref uint msgId, uint interval);
        delegate int PtStopPer(uint ch, uint msgId);

        [StructLayout(LayoutKind.Sequential)]
        struct Msg
        {
            public uint ProtocolID, RxStatus, TxFlags, Timestamp, DataSize, ExtraDataIndex;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4128)]
            public byte[] Data;
        }

        public void Load(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            SetDllDirectory(Path.GetDirectoryName(path));
            _dll = LoadLibraryEx(path, IntPtr.Zero, 8);
            if (_dll == IntPtr.Zero)
                throw new InvalidOperationException("LoadLibrary " + Marshal.GetLastWin32Error() + " " + path);
            _openFn = Get<PtOpen>("PassThruOpen");
            _closeFn = Get<PtClose>("PassThruClose");
            _connectFn = Get<PtConnect>("PassThruConnect");
            _discFn = Get<PtDisconnect>("PassThruDisconnect");
            _readFn = Get<PtRead>("PassThruReadMsgs");
            _writeFn = Get<PtWrite>("PassThruWriteMsgs");
            _filterFn = Get<PtFilter>("PassThruStartMsgFilter");
            _ioctlFn = Get<PtIoctl>("PassThruIoctl");
            _errFn = Get<PtErr>("PassThruGetLastError");
            _startPer = Get<PtStartPer>("PassThruStartPeriodicMsg");
            _stopPer = Get<PtStopPer>("PassThruStopPeriodicMsg");
        }

        public void OpenAndConnect()
        {
            uint id = 0;
            Check(_openFn(IntPtr.Zero, ref id), "Open");
            _dev = id; _open = true;
            uint ch = 0;
            Check(_connectFn(_dev, ISO15765, 0, 500000, ref ch), "Connect");
            _ch = ch; _conn = true;
            Ioctl(CLEAR_TX); Ioctl(CLEAR_RX); Ioctl(CLEAR_FILTERS);
            Filter(0x730, 0x630);
            StartTesterPresent();
            StartCan53F();
        }

        public void StartCan53F()
        {
            if (_per2On) return;
            uint ch = 0;
            Check(_connectFn(_dev, CAN, 0, 500000, ref ch), "ConnectCAN");
            _canCh = ch;
            _canConn = true;
            var m = NewMsg();
            m.ProtocolID = CAN;
            m.DataSize = 12;
            m.Data[2] = 0x05;
            m.Data[3] = 0x3F;
            m.Data[4] = 0x00;
            m.Data[5] = 0x40;
            m.Data[6] = 0xFF;
            m.Data[7] = 0xFF;
            m.Data[8] = 0xFF;
            m.Data[9] = 0xFF;
            m.Data[10] = 0xFF;
            m.Data[11] = 0xFF;
            IntPtr p = Ptr(m);
            try
            {
                uint id = 0;
                Check(_startPer(_canCh, p, ref id, 1000), "StartPeriodic53F");
                _perId2 = id;
                _per2On = true;
            }
            finally { Marshal.FreeHGlobal(p); }
        }

        public void StartTesterPresent()
        {
            if (_perOn) return;
            var m = NewMsg();
            m.TxFlags = PAD;
            m.DataSize = 6;
            uint can = 0x7DF | FUNC;
            m.Data[0] = (byte)(can >> 24);
            m.Data[1] = (byte)(can >> 16);
            m.Data[2] = (byte)(can >> 8);
            m.Data[3] = (byte)can;
            m.Data[4] = 0x3E;
            m.Data[5] = 0x80;
            IntPtr p = Ptr(m);
            try
            {
                uint id = 0;
                Check(_startPer(_ch, p, ref id, 1000), "StartPeriodic");
                _perId = id;
                _perOn = true;
            }
            finally { Marshal.FreeHGlobal(p); }
        }

        public void StopTesterPresent()
        {
            if (_perOn)
            {
                try { _stopPer(_ch, _perId); } catch { }
                _perOn = false;
            }
            if (_per2On)
            {
                try { _stopPer(_canCh, _perId2); } catch { }
                _per2On = false;
            }
            if (_canConn)
            {
                try { _discFn(_canCh); } catch { }
                _canConn = false;
            }
        }

        public void WritePhys(byte[] payload) { Write(0x730, payload, false); }
        public void WriteFunc(byte[] payload) { Write(0x7DF, payload, true); }
        public void ClearTx() { if (_conn) Ioctl(CLEAR_TX); }

        public byte[] Read(uint timeout)
        {
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Msg)));
            try
            {
                Marshal.StructureToPtr(NewMsg(), p, false);
                uint n = 1;
                int st = _readFn(_ch, p, ref n, timeout);
                if (st == 9 || st == 16 || n == 0) return null;
                Check(st, "Read");
                var m = (Msg)Marshal.PtrToStructure(p, typeof(Msg));
                if (m.DataSize <= 4) return new byte[0];
                var d = new byte[m.DataSize - 4];
                Buffer.BlockCopy(m.Data, 4, d, 0, d.Length);
                return d;
            }
            finally { Marshal.FreeHGlobal(p); }
        }

        void Write(uint id, byte[] payload, bool func)
        {
            var m = NewMsg();
            m.TxFlags = PAD;
            m.DataSize = (uint)(4 + payload.Length);
            uint can = func ? (id | FUNC) : id;
            m.Data[0] = (byte)(can >> 24); m.Data[1] = (byte)(can >> 16);
            m.Data[2] = (byte)(can >> 8); m.Data[3] = (byte)can;
            Buffer.BlockCopy(payload, 0, m.Data, 4, payload.Length);
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Msg)));
            try
            {
                Marshal.StructureToPtr(m, p, false);
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    uint n = 1;
                    int st = _writeFn(_ch, p, ref n, 2000);
                    if (st == 0) return;
                    if (st == 9 || st == 0x12)
                    {
                        try { Ioctl(CLEAR_TX); } catch { }
                        System.Threading.Thread.Sleep(20);
                        continue;
                    }
                    Check(st, "Write");
                }
                throw new InvalidOperationException("Write 0x9 ERR_TIMEOUT");
            }
            finally { Marshal.FreeHGlobal(p); }
        }

        void Filter(uint req, uint resp)
        {
            IntPtr mask = Ptr(Id(0xFFFFFFFF));
            IntPtr pat = Ptr(Id(resp));
            IntPtr flow = Ptr(Id(req));
            try
            {
                uint fid = 0;
                Check(_filterFn(_ch, FLOW, mask, pat, flow, ref fid), "Filter");
            }
            finally { Marshal.FreeHGlobal(mask); Marshal.FreeHGlobal(pat); Marshal.FreeHGlobal(flow); }
        }

        void Ioctl(uint id) { Check(_ioctlFn(_ch, id, IntPtr.Zero, IntPtr.Zero), "Ioctl"); }

        static Msg NewMsg() { return new Msg { ProtocolID = ISO15765, Data = new byte[4128] }; }
        static Msg Id(uint id)
        {
            var m = NewMsg();
            m.DataSize = 4; m.TxFlags = PAD;
            m.Data[0] = (byte)(id >> 24); m.Data[1] = (byte)(id >> 16);
            m.Data[2] = (byte)(id >> 8); m.Data[3] = (byte)id;
            return m;
        }
        static IntPtr Ptr(Msg m)
        {
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Msg)));
            Marshal.StructureToPtr(m, p, false);
            return p;
        }

        void Check(int st, string op)
        {
            if (st == 0) return;
            var sb = new StringBuilder(200);
            try { _errFn(sb); } catch { }
            throw new InvalidOperationException(op + " 0x" + st.ToString("X") + " " + sb);
        }

        T Get<T>(string name) where T : class
        {
            IntPtr p = GetProcAddress(_dll, name);
            if (p == IntPtr.Zero) throw new InvalidOperationException("nav " + name);
            return (T)(object)Marshal.GetDelegateForFunctionPointer(p, typeof(T));
        }

        public void Dispose()
        {
            try { StopTesterPresent(); } catch { }
            try { if (_conn) _discFn(_ch); } catch { }
            try { if (_open) _closeFn(_dev); } catch { }
            if (_dll != IntPtr.Zero) FreeLibrary(_dll);
            _dll = IntPtr.Zero; _conn = false; _open = false;
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)] static extern IntPtr LoadLibraryEx(string p, IntPtr f, uint flags);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)] static extern bool SetDllDirectory(string p);
        [DllImport("kernel32")] static extern bool FreeLibrary(IntPtr h);
        [DllImport("kernel32", CharSet = CharSet.Ansi)] static extern IntPtr GetProcAddress(IntPtr h, string n);
    }
}
