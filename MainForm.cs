using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DensoGen2
{
    public partial class MainForm : Form
    {
        J2534 _j;
        Ecu _ecu;
        CancellationTokenSource _cts;

        public MainForm()
        {
            InitializeComponent();
            _file.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "denso_08FF0000.bin");
        }

        void FormLoad(object sender, EventArgs e) { LoadDev(); }
        void RefreshClick(object sender, EventArgs e) { LoadDev(); }
        void ConnectClick(object sender, EventArgs e) { Toggle(); }
        void DidClick(object sender, EventArgs e) { Run(DoDids); }
        void ReadClick(object sender, EventArgs e) { Run(DoRead); }
        void WriteClick(object sender, EventArgs e) { Run(DoWrite); }
        void DevChanged(object sender, EventArgs e)
        {
            var d = _dev.SelectedItem as J2534DeviceInfo;
            if (d != null) _dll.Text = d.Library;
        }
        void BrowseClick(object sender, EventArgs e)
        {
            using (var d = new OpenFileDialog())
                if (d.ShowDialog() == DialogResult.OK) _file.Text = d.FileName;
        }

        void LoadDev()
        {
            _dev.Items.Clear();
            var list = J2534DeviceInfo.ListDevices();
            foreach (var d in list) _dev.Items.Add(d);
            if (_dev.Items.Count > 0) _dev.SelectedIndex = 0;
            else Log("J2534 reģistrā tukšs");
            Log("ierīces: " + list.Count);
        }

        void Toggle()
        {
            try
            {
                if (_j != null)
                {
                    _j.Dispose(); _j = null; _ecu = null;
                    _conn.Text = "Connect";
                    Log("disconnect");
                    return;
                }
                var info = _dev.SelectedItem as J2534DeviceInfo;
                string dll = info != null ? info.Library : _dll.Text;
                _j = new J2534();
                _j.Load(dll);
                _j.OpenAndConnect();
                _ecu = new Ecu(_j, Log, Ascii);
                _conn.Text = "Disconnect";
                Log("connect ISO15765 500k  TX 730  RX 630  3E 80 / 1000 ms  CAN 53F 00 40 FF FF FF FF FF FF / 1000 ms");
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                _j?.Dispose(); _j = null;
            }
        }

        async void Run(Func<CancellationToken, Task> act)
        {
            if (_ecu == null) { Log("vispirms Connect"); return; }
            _cts = new CancellationTokenSource();
            try { await act(_cts.Token); }
            catch (OperationCanceledException) { Log("stop"); }
            catch (Exception ex) { Log(ex.Message); }
        }

        async Task DoDids(CancellationToken ct)
        {
            await Task.Run(() => _ecu.ReadDids(), ct);
        }

        async Task DoRead(CancellationToken ct)
        {
            var pin = Ecu.Parse(_pin.Text);
            if (pin.Length != 5) throw new ArgumentException("PIN ir 5 baiti");
            int seeds = 1;
            int.TryParse(_seeds.Text.Trim(), out seeds);
            if (seeds < 1) seeds = 1;
            uint addr = Convert.ToUInt32(_addr.Text, 16);
            uint size = Convert.ToUInt32(_size.Text, 16);
            var prog = new Progress<int>(v => BeginInvoke(new Action(() => _bar.Value = Math.Min(100, v))));
            await Task.Run(() => _ecu.Read(addr, size, _file.Text, pin, seeds, prog, ct), ct);
            Log("saglabāts " + _file.Text);
        }

        async Task DoWrite(CancellationToken ct)
        {
            var pin = Ecu.Parse(_pin.Text);
            if (pin.Length != 5) throw new ArgumentException("PIN ir 5 baiti");
            int seeds = 1;
            int.TryParse(_seeds.Text.Trim(), out seeds);
            if (seeds < 1) seeds = 1;
            uint addr = Convert.ToUInt32(_addr.Text, 16);
            byte[] img = File.ReadAllBytes(_file.Text);
            var prog = new Progress<int>(v => BeginInvoke(new Action(() => _bar.Value = Math.Min(100, v))));
            await Task.Run(() => _ecu.Write(addr, img, pin, seeds, prog, ct), ct);
            Log("ierakstīts " + img.Length);
        }

        void Ascii(string s)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => Ascii(s))); return; }
            _ascii.AppendText(s + Environment.NewLine);
        }

        void Log(string s)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => Log(s))); return; }
            _log.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") + "  " + s + Environment.NewLine);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _j?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
