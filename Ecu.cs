using System;
using System.IO;
using System.Threading;

namespace DensoGen2
{
    /// <summary>
    /// Loga secība: 730/630, 7DF 3E 80 keep-alive tikai lasīšanas laikā, beigās 11 81.
    /// </summary>
    public sealed class Ecu
    {
        readonly J2534 _j;
        readonly Action<string> _log;
        readonly Action<string> _ascii;
        public int P2 = 250;
        public int P2Star = 5000;

        public Ecu(J2534 j, Action<string> log, Action<string> ascii = null)
        {
            _j = j;
            _log = log;
            _ascii = ascii ?? (_ => { });
        }

        public void TesterPresent()
        {
            _log("TX 7DF  3E 80");
            _j.WriteFunc(new byte[] { 0x3E, 0x80 });
        }

        public void Reset()
        {
            _log("TX 730  11 81");
            _j.WritePhys(new byte[] { 0x11, 0x81 });
            _log("TX 7DF  11 81");
            _j.WriteFunc(new byte[] { 0x11, 0x81 });
        }

        public void Unlock(byte[] pin5, int seedRepeats, CancellationToken ct)
        {
            Session(0x02);
            byte[] seed = null;
            for (int i = 0; i < seedRepeats; i++)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    seed = Seed(false);
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.IndexOf("TIMEOUT", StringComparison.OrdinalIgnoreCase) < 0 && ex.Message.IndexOf("0x9") < 0)
                        throw;
                    _log("write timeout, atkārtoju " + (i + 1));
                    try { _j.ClearTx(); } catch { }
                    Thread.Sleep(50);
                    i--;
                    continue;
                }
                if (i == 0 || (i + 1) % 1000 == 0 || i + 1 == seedRepeats)
                    _log("27 01  " + (i + 1) + "/" + seedRepeats + "  seed " + Hex(seed));
            }
            if (seed[0] == 0 && seed[1] == 0 && seed[2] == 0)
            {
                _log("seed 00 00 00, jau atvērts");
                return;
            }
            var key = SeedKey.P3Hash(pin5, seed);
            _log("PIN " + Hex(pin5) + "  P3Hash key " + Hex(key));
            var rx = Req(new byte[] { 0x27, 0x02, key[0], key[1], key[2] });
            if (rx.Length < 2 || rx[0] != 0x67 || rx[1] != 0x02)
                throw new InvalidOperationException("27 02 noraidīts " + Hex(rx));
            _log("SA01 OK");
        }

        public void Read(uint addr, uint size, string file, byte[] pin, int seedRepeats, IProgress<int> prog, CancellationToken ct)
        {
            Session(0x01);
            ReadDids();
            Unlock(pin, seedRepeats, ct);
            int max = Upload(addr, size);
            int chunk = 0x1800;
            if (max > 2 && max - 2 < chunk) chunk = max - 2;
            using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                uint left = size;
                byte bsc = 1;
                uint done = 0;
                while (left > 0)
                {
                    ct.ThrowIfCancellationRequested();
                    var block = Transfer(bsc);
                    int take = (int)Math.Min(left, (uint)block.Length);
                    fs.Write(block, 0, take);
                    left -= (uint)take;
                    done += (uint)take;
                    prog.Report((int)(done * 100 / size));
                    _log("36 " + bsc.ToString("X2") + "  " + take + "  left " + left);
                    bsc++;
                }
            }
            Reset();
        }

        public void Write(uint addr, byte[] image, byte[] pin, int seedRepeats, IProgress<int> prog, CancellationToken ct)
        {
            Session(0x01);
            Unlock(pin, seedRepeats, ct);
            int max = Download(addr, (uint)image.Length);
            int chunk = max > 2 ? max - 2 : 0x800;
            int off = 0;
            byte bsc = 1;
            while (off < image.Length)
            {
                ct.ThrowIfCancellationRequested();
                int n = Math.Min(chunk, image.Length - off);
                var tx = new byte[2 + n];
                tx[0] = 0x36; tx[1] = bsc;
                Buffer.BlockCopy(image, off, tx, 2, n);
                var rx = Req(tx);
                if (rx.Length < 2 || rx[0] != 0x76 || rx[1] != bsc)
                    throw new InvalidOperationException("36 noraidīts " + Hex(rx));
                off += n;
                prog.Report(off * 100 / image.Length);
                _log("36 " + bsc.ToString("X2") + "  " + n);
                bsc++;
            }
            Req(new byte[] { 0x37 });
            Reset();
        }

        public void ReadDids()
        {
            ushort[] dids = { 0xF190, 0xF186, 0xF12A, 0xF12B, 0xF12E, 0xF1F0, 0xF194, 0xF120, 0xF121, 0xF125, 0xF196, 0xF18C };
            foreach (var d in dids)
            {
                var rx = Req(new byte[] { 0x22, (byte)(d >> 8), (byte)d });
                _log("DID " + d.ToString("X4") + " " + Hex(rx));
                _ascii(AsciiDid(d, rx));
            }
        }

        public static string AsciiDid(ushort did, byte[] rx)
        {
            if (rx == null || rx.Length < 3 || rx[0] != 0x62)
                return did.ToString("X4") + "  " + Hex(rx);
            int off = 3;
            if (did == 0xF120 || did == 0xF121 || did == 0xF125 || did == 0xF18C || did == 0xF12A || did == 0xF12B || did == 0xF12E)
            {
                var payload = Slice(rx, off);
                return did.ToString("X4") + "  " + DidName(did) + "  " + Pack(StripSpace(payload)) + Suffix(payload);
            }
            var sb = new System.Text.StringBuilder();
            sb.Append(did.ToString("X4")).Append("  ").Append(DidName(did)).Append("  ");
            for (int i = off; i < rx.Length; i++)
            {
                byte c = rx[i];
                sb.Append(c >= 32 && c < 127 ? (char)c : '.');
            }
            return sb.ToString();
        }

        static string Pack(byte[] d)
        {
            var sb = new System.Text.StringBuilder(d.Length * 2);
            for (int i = 0; i < d.Length; i++) sb.Append(d[i].ToString("X2"));
            return sb.ToString();
        }

        static byte[] StripSpace(byte[] d)
        {
            int n = 0;
            for (int i = 0; i < d.Length; i++) if (d[i] != 0x20) n++;
            var o = new byte[n];
            int j = 0;
            for (int i = 0; i < d.Length; i++) if (d[i] != 0x20) o[j++] = d[i];
            return o;
        }

        static string Suffix(byte[] d)
        {
            if (d.Length < 3) return "";
            int n = d.Length;
            if (d[n - 3] == 0x20 && d[n - 2] >= 0x41 && d[n - 2] <= 0x5A && d[n - 1] >= 0x41 && d[n - 1] <= 0x5A)
                return "  [" + (char)d[n - 2] + (char)d[n - 1] + "]";
            return "";
        }

        static byte[] Slice(byte[] src, int off)
        {
            var d = new byte[src.Length - off];
            Buffer.BlockCopy(src, off, d, 0, d.Length);
            return d;
        }

        static string DidName(ushort d)
        {
            switch (d)
            {
                case 0xF190: return "VIN";
                case 0xF186: return "sesija";
                case 0xF18C: return "serial";
                case 0xF194: return "soft";
                case 0xF120: return "sw-id";
                case 0xF121: return "sw-id2";
                case 0xF125: return "sw-id3";
                case 0xF196: return "shop";
                case 0xF12A: return "ecu";
                case 0xF12B: return "hw";
                case 0xF12E: return "sw";
                case 0xF1F0: return "diag";
                default: return "";
            }
        }

        void Session(byte s)
        {
            var rx = Req(new byte[] { 0x10, s });
            if (rx.Length < 2 || rx[0] != 0x50 || rx[1] != s)
                throw new InvalidOperationException("10 " + s.ToString("X2") + " " + Hex(rx));
            if (rx.Length >= 6)
            {
                P2 = Math.Max(50, (rx[2] << 8) | rx[3]);
                int star = ((rx[4] << 8) | rx[5]) * 10;
                if (star > 0) P2Star = star;
                _log("P2=" + P2 + " P2*=" + P2Star);
            }
        }

        byte[] Seed(bool log)
        {
            var rx = Req(new byte[] { 0x27, 0x01 }, log);
            if (rx.Length < 5 || rx[0] != 0x67 || rx[1] != 0x01)
                throw new InvalidOperationException("27 01 " + Hex(rx));
            var seed = new byte[] { rx[2], rx[3], rx[4] };
            if (log) _log("seed " + Hex(seed));
            return seed;
        }

        int Upload(uint addr, uint size)
        {
            var rx = Req(Addr(0x35, addr, size));
            if (rx.Length < 4 || rx[0] != 0x75)
                throw new InvalidOperationException("35 " + Hex(rx));
            return (rx[2] << 8) | rx[3];
        }

        int Download(uint addr, uint size)
        {
            var rx = Req(Addr(0x34, addr, size));
            if (rx.Length < 4 || rx[0] != 0x74)
                throw new InvalidOperationException("34 " + Hex(rx));
            return (rx[2] << 8) | rx[3];
        }

        byte[] Transfer(byte bsc)
        {
            var rx = Req(new byte[] { 0x36, bsc });
            if (rx.Length < 2 || rx[0] != 0x76 || rx[1] != bsc)
                throw new InvalidOperationException("36 " + Hex(rx));
            var d = new byte[rx.Length - 2];
            Buffer.BlockCopy(rx, 2, d, 0, d.Length);
            return d;
        }

        static byte[] Addr(byte sid, uint addr, uint size)
        {
            return new byte[]
            {
                sid, 0x00, 0x44,
                (byte)(addr >> 24), (byte)(addr >> 16), (byte)(addr >> 8), (byte)addr,
                (byte)(size >> 24), (byte)(size >> 16), (byte)(size >> 8), (byte)size
            };
        }

        byte[] Req(byte[] payload) { return Req(payload, true); }

        byte[] Req(byte[] payload, bool log)
        {
            if (log) _log("TX 730  " + Hex(payload));
            _j.WritePhys(payload);
            var until = DateTime.UtcNow.AddMilliseconds(P2Star);
            while (DateTime.UtcNow < until)
            {
                var rx = _j.Read((uint)P2);
                if (rx == null || rx.Length == 0) continue;
                if (log) _log("RX 630  " + Hex(rx));
                if (rx[0] == 0x7F && rx.Length >= 3 && rx[2] == 0x78)
                {
                    until = DateTime.UtcNow.AddMilliseconds(P2Star);
                    continue;
                }
                if (rx[0] == 0x7F && rx.Length >= 3)
                    throw new InvalidOperationException("NRC " + rx[1].ToString("X2") + " " + rx[2].ToString("X2"));
                return rx;
            }
            throw new TimeoutException("nav atbildes " + Hex(payload));
        }

        public static string Hex(byte[] d)
        {
            if (d == null) return "";
            var p = new string[d.Length];
            for (int i = 0; i < d.Length; i++) p[i] = d[i].ToString("X2");
            return string.Join(" ", p);
        }

        public static byte[] Parse(string text)
        {
            if (text == null) throw new ArgumentException("PIN tukšs");
            string hex = text.Trim().Replace(" ", "").Replace(",", "");
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex.Substring(2);
            if (hex.Length == 0 || (hex.Length % 2) != 0)
                throw new ArgumentException("PIN hexs nav pāra garumā: " + text);
            var b = new byte[hex.Length / 2];
            for (int i = 0; i < b.Length; i++)
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return b;
        }
    }
}
