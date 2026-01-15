//using System.Drawing;
//using System.Windows.Forms;
//using AntiCheat.Core;

//namespace AntiCheat
//{
//    public class MainForm : Form
//    {
//        private Label mtaStatus;
//        private Label serialLabel;
//        private Label hwidLabel;
//        private Timer updateTimer;

//        public MainForm()
//        {
//            this.Text = "AntiCheat";
//            this.Width = 500;
//            this.Height = 300;
//            this.StartPosition = FormStartPosition.CenterScreen;
//            this.FormBorderStyle = FormBorderStyle.FixedDialog;
//            this.MaximizeBox = false;

//            Panel sidePanel = new Panel()
//            {
//                Dock = DockStyle.Right,
//                Width = 220,
//                BackColor = Color.FromArgb(240, 240, 240)
//            };

//            mtaStatus = new Label()
//            {
//                Text = "MTA:SA not running",
//                ForeColor = Color.Red,
//                AutoSize = true,
//                Location = new Point(15, 20)
//            };

//            serialLabel = new Label()
//            {
//                Text = "Connected to serial ID: ----",
//                AutoSize = true,
//                Location = new Point(15, 60)
//            };

//            hwidLabel = new Label()
//            {
//                Text = "HWID ID: " + HwidGenerator.GetHwid(),
//                AutoSize = true,
//                Location = new Point(15, 100)
//            };

//            sidePanel.Controls.Add(mtaStatus);
//            sidePanel.Controls.Add(serialLabel);
//            sidePanel.Controls.Add(hwidLabel);

//            this.Controls.Add(sidePanel);

//            updateTimer = new Timer();
//            updateTimer.Interval = 2000;
//            updateTimer.Tick += UpdateTimer_Tick;
//            updateTimer.Start();
//        }

//        private void UpdateTimer_Tick(object sender, System.EventArgs e)
//        {
//            if (ProcessScanner.IsMtaRunning())
//            {
//                mtaStatus.Text = "MTA:SA running";
//                mtaStatus.ForeColor = Color.Green;

//                string serial = SerialGrabber.GetMTASerial();
//                if (serial != null)
//                {
//                    serialLabel.Text = "Connected to serial ID: " + serial;
//                }
//                else
//                {
//                    serialLabel.Text = "Connected to serial ID: NOT FOUND";
//                }
//            }
//            else
//            {
//                mtaStatus.Text = "MTA:SA not running";
//                mtaStatus.ForeColor = Color.Red;
//                serialLabel.Text = "Connected to serial ID: ----";
//            }
//        }

//    }
//}

using System;
using System.Drawing;
using System.Windows.Forms;
using AntiCheat.Core;

namespace AntiCheat
{
    public class MainForm : Form
    {
        private Label mtaStatus;
        private Label serialLabel;
        private Label hwidLabel;
        private Label sessionLabel;
        private Timer updateTimer;

        private Timer heartbeatTimer;


        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private bool sessionSent = false;
        private string sessionToken = null;

        public MainForm()
        {
            // FORM SETTINGS
            this.Text = "AntiCheat";
            this.Width = 500;
            this.Height = 300;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ShowInTaskbar = false;

            // SIDE PANEL
            Panel sidePanel = new Panel()
            {
                Dock = DockStyle.Right,
                Width = 220,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            mtaStatus = new Label()
            {
                Text = "MTA:SA not running",
                ForeColor = Color.Red,
                AutoSize = true,
                Location = new Point(15, 20)
            };

            serialLabel = new Label()
            {
                Text = "Connected to serial ID: ----",
                AutoSize = true,
                Location = new Point(15, 60)
            };

            hwidLabel = new Label()
            {
                Text = "HWID ID: " + HwidGenerator.GetHwid(),
                AutoSize = true,
                Location = new Point(15, 100)
            };

            sessionLabel = new Label()
            {
                Text = "Session: Not registered",
                AutoSize = true,
                Location = new Point(15, 140)
            };

            sidePanel.Controls.Add(mtaStatus);
            sidePanel.Controls.Add(serialLabel);
            sidePanel.Controls.Add(hwidLabel);
            sidePanel.Controls.Add(sessionLabel);
            this.Controls.Add(sidePanel);

            // TIMER
            updateTimer = new Timer();
            updateTimer.Interval = 2000;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            // TRAY
            InitializeTray();

            // START MINIMIZED
            //this.Load += (s, e) => this.Hide();
        }

        private void InitializeTray()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open", null, (s, e) => ShowApp());
            trayMenu.Items.Add("Exit", null, (s, e) => ExitApp());

            trayIcon = new NotifyIcon()
            {
                Text = "AntiCheat",
                Icon = this.Icon,
                ContextMenuStrip = trayMenu,
                Visible = true
            };

            trayIcon.DoubleClick += (s, e) => ShowApp();
        }

        private void ShowApp()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void ExitApp()
        {
            var result = MessageBox.Show(
                "Warning:\nIf you close Spectre-AntiCheat while in a protected game, you will be disconnected.",
                "Spectre-AntiCheat",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes)
                return;

            trayIcon.Visible = false;

            heartbeatTimer?.Stop();
            Application.Exit();
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var result = MessageBox.Show(
                    "Warning:\nIf you close Spectre-AntiCheat while in a protected game, you will be disconnected.",
                    "Spectre-AntiCheat",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                trayIcon.Visible = false;
            }

            base.OnFormClosing(e);
        }

        private void StartHeartbeat()
        {
            heartbeatTimer = new Timer();
            heartbeatTimer.Interval = 5000; // 5 seconds
            heartbeatTimer.Tick += async (s, e) =>
            {
                if (string.IsNullOrEmpty(sessionToken))
                    return;

                bool ok = await ApiClient.SendHeartbeatAsync(sessionToken, SerialGrabber.GetMTASerial(), HwidGenerator.GetHwid());

                if (!ok)
                {
                    MessageBox.Show(
                        "Connection to anti-cheat server lost.\nYou will be disconnected.",
                        "Spectre-AntiCheat",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    Application.Exit();
                }
            };

            heartbeatTimer.Start();
        }


        private async void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (!ProcessScanner.IsMtaRunning())
            {
                mtaStatus.Text = "MTA:SA not running";
                mtaStatus.ForeColor = Color.Red;
                serialLabel.Text = "Connected to serial ID: ----";
                return;
            }

            mtaStatus.Text = "MTA:SA running";
            mtaStatus.ForeColor = Color.Green;

            string serial = SerialGrabber.GetMTASerial();

            if (string.IsNullOrEmpty(serial))
            {
                serialLabel.Text = "Connected to serial ID: NOT FOUND";
                return;
            }

            serialLabel.Text = "Connected to serial ID: " + serial;

            if (sessionSent)
                return;

            sessionSent = true;

            try
            {
                var response = await ApiClient.RegisterSessionAsync(
                    serial,
                    HwidGenerator.GetHwid()
                );

                sessionToken = response.session_token;
                sessionLabel.Text = "Session: " + response.status;

                StartHeartbeat();
                if (response.status == "modified")
                {
                    MessageBox.Show(
                        "Security warning:\nSystem modification detected.",
                        "AntiCheat",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch
            {
                sessionSent = false;
            }
        }
    }
}

