namespace DensoGen2
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblIface = new System.Windows.Forms.Label();
            this._dev = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this._conn = new System.Windows.Forms.Button();
            this.lblDll = new System.Windows.Forms.Label();
            this._dll = new System.Windows.Forms.TextBox();
            this.lblPin = new System.Windows.Forms.Label();
            this._pin = new System.Windows.Forms.TextBox();
            this.lblSeeds = new System.Windows.Forms.Label();
            this._seeds = new System.Windows.Forms.TextBox();
            this.lblAddr = new System.Windows.Forms.Label();
            this._addr = new System.Windows.Forms.TextBox();
            this.lblSize = new System.Windows.Forms.Label();
            this._size = new System.Windows.Forms.TextBox();
            this.btnDid = new System.Windows.Forms.Button();
            this.btnRead = new System.Windows.Forms.Button();
            this.btnWrite = new System.Windows.Forms.Button();
            this.lblFile = new System.Windows.Forms.Label();
            this._file = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this._bar = new System.Windows.Forms.ProgressBar();
            this.lblCan = new System.Windows.Forms.Label();
            this._log = new System.Windows.Forms.TextBox();
            this.lblAscii = new System.Windows.Forms.Label();
            this._ascii = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblIface
            // 
            this.lblIface.AutoSize = true;
            this.lblIface.Location = new System.Drawing.Point(12, 14);
            this.lblIface.Name = "lblIface";
            this.lblIface.Size = new System.Drawing.Size(55, 15);
            this.lblIface.Text = "Interfeiss";
            // 
            // _dev
            // 
            this._dev.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._dev.Location = new System.Drawing.Point(88, 10);
            this._dev.Name = "_dev";
            this._dev.Size = new System.Drawing.Size(560, 23);
            this._dev.SelectedIndexChanged += new System.EventHandler(this.DevChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(656, 8);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(80, 25);
            this.btnRefresh.Text = "Atjaunot";
            this.btnRefresh.Click += new System.EventHandler(this.RefreshClick);
            // 
            // _conn
            // 
            this._conn.Location = new System.Drawing.Point(744, 8);
            this._conn.Name = "_conn";
            this._conn.Size = new System.Drawing.Size(90, 25);
            this._conn.Text = "Connect";
            this._conn.Click += new System.EventHandler(this.ConnectClick);
            // 
            // lblDll
            // 
            this.lblDll.AutoSize = true;
            this.lblDll.Location = new System.Drawing.Point(12, 44);
            this.lblDll.Name = "lblDll";
            this.lblDll.Size = new System.Drawing.Size(26, 15);
            this.lblDll.Text = "DLL";
            // 
            // _dll
            // 
            this._dll.Location = new System.Drawing.Point(88, 40);
            this._dll.Name = "_dll";
            this._dll.ReadOnly = true;
            this._dll.Size = new System.Drawing.Size(640, 23);
            // 
            // lblPin
            // 
            this.lblPin.AutoSize = true;
            this.lblPin.Location = new System.Drawing.Point(12, 76);
            this.lblPin.Name = "lblPin";
            this.lblPin.Size = new System.Drawing.Size(46, 15);
            this.lblPin.Text = "PIN 5B";
            // 
            // _pin
            // 
            this._pin.Location = new System.Drawing.Point(88, 72);
            this._pin.Name = "_pin";
            this._pin.MaxLength = 10;
            this._pin.Size = new System.Drawing.Size(110, 23);
            //
            // lblSeeds
            //
            this.lblSeeds.AutoSize = true;
            this.lblSeeds.Location = new System.Drawing.Point(200, 76);
            this.lblSeeds.Name = "lblSeeds";
            this.lblSeeds.Size = new System.Drawing.Size(36, 15);
            this.lblSeeds.Text = "Seed";
            //
            // _seeds
            //
            this._seeds.Location = new System.Drawing.Point(236, 72);
            this._seeds.Name = "_seeds";
            this._seeds.Size = new System.Drawing.Size(52, 23);
            this._seeds.Text = "1";
            // 
            // lblAddr
            // 
            this.lblAddr.AutoSize = true;
            this.lblAddr.Location = new System.Drawing.Point(240, 76);
            this.lblAddr.Name = "lblAddr";
            this.lblAddr.Size = new System.Drawing.Size(42, 15);
            this.lblAddr.Text = "Adrese";
            // 
            // _addr
            // 
            this._addr.Location = new System.Drawing.Point(292, 72);
            this._addr.Name = "_addr";
            this._addr.Size = new System.Drawing.Size(90, 23);
            this._addr.Text = "08FF0000";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(392, 76);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(42, 15);
            this.lblSize.Text = "Garums";
            // 
            // _size
            // 
            this._size.Location = new System.Drawing.Point(444, 72);
            this._size.Name = "_size";
            this._size.Size = new System.Drawing.Size(90, 23);
            this._size.Text = "00390000";
            // 
            // btnDid
            // 
            this.btnDid.Location = new System.Drawing.Point(560, 70);
            this.btnDid.Name = "btnDid";
            this.btnDid.Size = new System.Drawing.Size(80, 28);
            this.btnDid.Text = "DID";
            this.btnDid.Click += new System.EventHandler(this.DidClick);
            // 
            // btnRead
            // 
            this.btnRead.Location = new System.Drawing.Point(648, 70);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(80, 28);
            this.btnRead.Text = "Read";
            this.btnRead.Click += new System.EventHandler(this.ReadClick);
            // 
            // btnWrite
            // 
            this.btnWrite.Location = new System.Drawing.Point(736, 70);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(80, 28);
            this.btnWrite.Text = "Write";
            this.btnWrite.Click += new System.EventHandler(this.WriteClick);
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(12, 108);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(28, 15);
            this.lblFile.Text = "Fails";
            // 
            // _file
            // 
            this._file.Location = new System.Drawing.Point(88, 104);
            this._file.Name = "_file";
            this._file.Size = new System.Drawing.Size(640, 23);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(736, 102);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(36, 25);
            this.btnBrowse.Text = "...";
            this.btnBrowse.Click += new System.EventHandler(this.BrowseClick);
            // 
            // _bar
            // 
            this._bar.Location = new System.Drawing.Point(12, 136);
            this._bar.Name = "_bar";
            this._bar.Size = new System.Drawing.Size(1060, 16);
            // 
            // lblCan
            // 
            this.lblCan.AutoSize = true;
            this.lblCan.Location = new System.Drawing.Point(12, 156);
            this.lblCan.Name = "lblCan";
            this.lblCan.Size = new System.Drawing.Size(30, 15);
            this.lblCan.Text = "CAN";
            // 
            // _log
            // 
            this._log.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this._log.Font = new System.Drawing.Font("Consolas", 9F);
            this._log.Location = new System.Drawing.Point(12, 174);
            this._log.Multiline = true;
            this._log.Name = "_log";
            this._log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._log.Size = new System.Drawing.Size(640, 400);
            // 
            // lblAscii
            // 
            this.lblAscii.AutoSize = true;
            this.lblAscii.Location = new System.Drawing.Point(664, 156);
            this.lblAscii.Name = "lblAscii";
            this.lblAscii.Size = new System.Drawing.Size(78, 15);
            this.lblAscii.Text = "22 DID ASCII";
            // 
            // _ascii
            // 
            this._ascii.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this._ascii.Font = new System.Drawing.Font("Consolas", 9F);
            this._ascii.Location = new System.Drawing.Point(664, 174);
            this._ascii.Multiline = true;
            this._ascii.Name = "_ascii";
            this._ascii.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._ascii.Size = new System.Drawing.Size(400, 400);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 590);
            this.Controls.Add(this.lblIface);
            this.Controls.Add(this._dev);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this._conn);
            this.Controls.Add(this.lblDll);
            this.Controls.Add(this._dll);
            this.Controls.Add(this.lblPin);
            this.Controls.Add(this._pin);
            this.Controls.Add(this.lblSeeds);
            this.Controls.Add(this._seeds);
            this.Controls.Add(this.lblAddr);
            this.Controls.Add(this._addr);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this._size);
            this.Controls.Add(this.btnDid);
            this.Controls.Add(this.btnRead);
            this.Controls.Add(this.btnWrite);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this._file);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this._bar);
            this.Controls.Add(this.lblCan);
            this.Controls.Add(this._log);
            this.Controls.Add(this.lblAscii);
            this.Controls.Add(this._ascii);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Denso Gen2";
            this.Load += new System.EventHandler(this.FormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblIface;
        private System.Windows.Forms.ComboBox _dev;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button _conn;
        private System.Windows.Forms.Label lblDll;
        private System.Windows.Forms.TextBox _dll;
        private System.Windows.Forms.Label lblPin;
        private System.Windows.Forms.TextBox _pin;
        private System.Windows.Forms.Label lblSeeds;
        private System.Windows.Forms.TextBox _seeds;
        private System.Windows.Forms.Label lblAddr;
        private System.Windows.Forms.TextBox _addr;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.TextBox _size;
        private System.Windows.Forms.Button btnDid;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.Button btnWrite;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.TextBox _file;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.ProgressBar _bar;
        private System.Windows.Forms.Label lblCan;
        private System.Windows.Forms.TextBox _log;
        private System.Windows.Forms.Label lblAscii;
        private System.Windows.Forms.TextBox _ascii;
    }
}
