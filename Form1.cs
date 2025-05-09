using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace RefreshRateSwitch
{
    /// <summary>
    /// Main application form that provides a system tray utility for quickly switching
    /// between monitor refresh rates. Features a simple UI with a toggle for startup settings.
    /// </summary>
    public partial class Form1 : Form
    {
        #region Private Fields

        private bool startOnStartup = false;
        private NotifyIcon trayIcon;

        // WinAPI Constants for display settings
        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int DM_DISPLAYFREQUENCY = 0x400000;
        private const int CDS_UPDATEREGISTRY = 0x01;

        #endregion

        #region Constructor & Initialization

        /// <summary>
        /// Initializes the form and configures tray icon, refresh rate detection,
        /// and startup settings.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            // Position window near system tray on startup
            this.Load += (s, e) => ShowNearTray();

            InitializeRefreshRates();
            InitializeTrayIcon();
            LoadStartupSetting();
        }

        /// <summary>
        /// Initializes available refresh rates and sets current rate
        /// </summary>
        private void InitializeRefreshRates()
        {
            // Get all available refresh rates for the display
            availableRefreshRates = GetAvailableRefreshRates();
            availableRefreshRates.Sort(); // Ensure they are in ascending order

            // Detect current refresh rate
            int currentHz = GetCurrentRefreshRate();
            currentRateIndex = availableRefreshRates.IndexOf(currentHz);

            // Handle case where current rate isn't in the detected list
            if (currentRateIndex == -1)
            {
                availableRefreshRates.Add(currentHz); // Add as fallback if current not detected
                availableRefreshRates.Sort();
                currentRateIndex = availableRefreshRates.IndexOf(currentHz);
            }

            // Update the UI with current rate
            UpdateHzLabel(currentHz);
        }

        /// <summary>
        /// Sets up the system tray icon and its context menu
        /// </summary>
        private void InitializeTrayIcon()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                using (Stream iconStream = asm.GetManifestResourceStream("RefreshRateSwitch.RefreshRateSwitchIcon.ico"))
                {
                    if (iconStream == null)
                        throw new Exception("Embedded icon not found. Double-check the resource name and that Build Action is 'Embedded Resource'.");

                    Icon icon = new Icon(iconStream);

                    this.Icon = icon;

                    trayIcon = new NotifyIcon()
                    {
                        Icon = icon,
                        Visible = true,
                        Text = "Toggle Refresh Rate"
                    };
                }

                // Create context menu for tray icon
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Toggle Refresh Rate", null, ToggleRefreshRate);
                contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

                trayIcon.ContextMenuStrip = contextMenu;
                trayIcon.DoubleClick += (s, e) => ToggleRefreshRate(s, e);
                trayIcon.MouseClick += TrayIcon_MouseClick;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing tray icon: {ex.Message}",
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Startup Settings Management

        /// <summary>
        /// Toggles whether the application starts with Windows
        /// </summary>
        private void ToggleStartupSetting(object sender, EventArgs e)
        {
            startOnStartup = !startOnStartup;
            SaveStartupSetting();
            UpdateStartupToggleUI();
        }

        /// <summary>
        /// Updates the toggle UI to reflect current startup setting
        /// </summary>
        private void UpdateStartupToggleUI()
        {
            // Clear any existing controls first
            startupToggle.Controls.Clear();

            // Create the toggle knob
            Panel knob = new Panel
            {
                Size = new Size(20, 20),
                BackColor = Color.White,
                Location = new Point(startOnStartup ? 25 : 5, 2)
            };

            // Make the knob clicks pass through to the parent panel
            knob.Click += (s, e) => ToggleStartupSetting(startupToggle, e);

            // Add the knob to the startupToggle panel
            startupToggle.Controls.Add(knob);

            // Change background color based on state
            startupToggle.BackColor = startOnStartup ? Color.LightGreen : Color.LightGray;
        }

        /// <summary>
        /// Loads the startup setting from Windows registry
        /// </summary>
        private void LoadStartupSetting()
        {
            try
            {
                string appName = "RefreshRateSwitch";
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
                {
                    if (key != null)
                    {
                        string value = key.GetValue(appName) as string;
                        startOnStartup = value == Application.ExecutablePath;
                    }
                    else
                    {
                        startOnStartup = false;
                    }
                    UpdateStartupToggleUI();
                }
            }
            catch (Exception ex)
            {
                // Handle any registry access errors
                MessageBox.Show("Error accessing startup settings: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                startOnStartup = false;
                UpdateStartupToggleUI();
            }
        }

        /// <summary>
        /// Saves the startup setting to Windows registry
        /// </summary>
        private void SaveStartupSetting()
        {
            string appName = "RefreshRateSwitch";
            string exePath = Application.ExecutablePath;

            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        if (startOnStartup)
                            key.SetValue(appName, exePath);
                        else
                            key.DeleteValue(appName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving startup setting: {ex.Message}",
                    "Registry Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Verifies if the application is properly registered to run at startup
        /// </summary>
        private bool VerifyStartupSetting()
        {
            try
            {
                string appName = "RefreshRateSwitch";
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
                {
                    if (key != null)
                    {
                        string value = key.GetValue(appName) as string;
                        return value == Application.ExecutablePath;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Refresh Rate Management

        /// <summary>
        /// Gets all available refresh rates for the current display
        /// </summary>
        private List<int> GetAvailableRefreshRates()
        {
            HashSet<int> refreshRates = new HashSet<int>();
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            int modeIndex = 0;
            while (EnumDisplaySettings(null, modeIndex, ref devMode))
            {
                if (devMode.dmDisplayFrequency > 0)
                    refreshRates.Add(devMode.dmDisplayFrequency);
                modeIndex++;
            }

            return new List<int>(refreshRates);
        }

        /// <summary>
        /// Cycles to the next available refresh rate
        /// </summary>
        private void ToggleRefreshRate(object sender, EventArgs e)
        {
            // Temporarily disable the Hz label and change its color to indicate switching
            HzLabel.Enabled = false;
            HzLabel.BackColor = Color.Gray;

            // Calculate the next refresh rate in the cycle
            currentRateIndex = (currentRateIndex + 1) % availableRefreshRates.Count;
            int targetRate = availableRefreshRates[currentRateIndex];

            // Use a background thread to change the refresh rate
            Task.Run(() =>
            {
                SetRefreshRate(targetRate);

                // Update UI on the UI thread when done
                Invoke(new Action(() =>
                {
                    UpdateHzLabel(targetRate);
                    HzLabel.Enabled = true;
                }));
            });
        }

        /// <summary>
        /// Changes the display's refresh rate to the specified value
        /// </summary>
        private void SetRefreshRate(int hz)
        {
            DEVMODE vDevMode = new DEVMODE();
            vDevMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref vDevMode))
            {
                vDevMode.dmDisplayFrequency = hz;
                vDevMode.dmFields = DM_DISPLAYFREQUENCY;
                ChangeDisplaySettings(ref vDevMode, CDS_UPDATEREGISTRY);
            }
        }

        /// <summary>
        /// Gets the current refresh rate of the display
        /// </summary>
        private int GetCurrentRefreshRate()
        {
            DEVMODE vDevMode = new DEVMODE();
            vDevMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref vDevMode))
            {
                return vDevMode.dmDisplayFrequency;
            }

            return -1; // Fallback if unable to determine current rate
        }

        /// <summary>
        /// Updates the Hz label with the current refresh rate and color-codes based on rate
        /// </summary>
        private void UpdateHzLabel(int hz)
        {
            HzLabel.Text = $"[{hz}Hz]";

            // Color coding based on refresh rate (lower = more orange, higher = more green)
            int minHz = availableRefreshRates.Min();
            int maxHz = availableRefreshRates.Max();

            // Normalize hz to a 0–1 range
            float t = (float)(hz - minHz) / (maxHz - minHz);

            // Orange (255, 165, 0) to Green (0, 255, 0) color gradient
            int r = (int)(255 * (1 - t));       // Red goes from 255 to 0
            int g = (int)(165 + (90 * t));      // Green goes from 165 to 255
            int b = 0;                           // Blue stays 0

            HzLabel.BackColor = Color.FromArgb(r, g, b);
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Handles clicking on the Hz label
        /// </summary>
        private void HzLabel_Click(object sender, EventArgs e)
        {
            ToggleRefreshRate(null, null);
        }

        /// <summary>
        /// Handles mouse clicks on the tray icon
        /// </summary>
        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Show the form above the tray icon
                ShowNearTray();
            }
        }

        /// <summary>
        /// Shows the form near the system tray
        /// </summary>
        private void ShowNearTray()
        {
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;

            // Position in bottom right corner, near the system tray
            int x = workingArea.Right - this.Width - 10;
            int y = workingArea.Bottom - this.Height - 10;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(x, y);

            this.Show();
            this.BringToFront();
            this.Activate();
        }

        /// <summary>
        /// Hides the form when it loses focus
        /// </summary>
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            this.Hide();
        }

        #endregion

        #region WinAPI Methods

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        /// <summary>
        /// Structure for display device settings used by Windows API
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;

            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;

            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        #endregion
    }
}