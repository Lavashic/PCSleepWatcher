using Newtonsoft.Json;
using Thread = System.Threading.Thread;
using Timer = System.Threading.Timer;

namespace PCSleepWatcher
{
    internal class TrayApp : Form
    {
        private readonly PipeServer _pipeServer;
        private readonly SettingsService _settings;
        private readonly ToolStripMenuItem _toggleItem;
        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenuStrip _trayMenu;
        private readonly string _version;
        private const string AppName = "НеБуди";
        private Timer _serviceTimer;

        private TrayApp()
        {
            _settings = new SettingsService();
            _settings.LoadState();
            _pipeServer = new PipeServer(_settings);
            _version = GetVersion();
            var pipeServerThread = new Thread(_pipeServer.StartPipeServer)
            {
                IsBackground = true
            };
            pipeServerThread.Start();
            _pipeServer.StateChanged += HandleStateChanged;
            InitializeTimer();

            _trayMenu = new ContextMenuStrip();
            _toggleItem = new ToolStripMenuItem("", null, OnToggleClick);
            _trayMenu.Items.Add(_toggleItem);
            _trayMenu.Items.Add("Выйти", null, OnExit);
            var appName = string.IsNullOrWhiteSpace(_version) ? AppName : $"{AppName} ({_version})";
            _trayIcon = new NotifyIcon
            {
                Text = $"{appName} (Интервал: {_settings.GetTimeInterval()})",
                ContextMenuStrip = _trayMenu,
                Visible = true
            };
            UpdateState();
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Logger.LogError($"Unhandled Exception: {e.ExceptionObject}\n");
            };

            Application.ThreadException += (s, e) =>
            {
                Logger.LogError($"Thread Exception: {e.Exception.Message}\n");
            };
        }

        [STAThread]
        private static async Task Main()
        {
            var trayApp = new TrayApp();
            Application.Run(trayApp);
        }

        private void HandleStateChanged(object? sender, EventArgs e)
        {
            UpdateState();
        }

        private void InitializeTimer()
        {
            _serviceTimer = new Timer(
                ServiceCallback,
                null,
                _settings.IsOn ? 0 : Timeout.Infinite,
                _settings.IsOn ? _settings.IntervalSeconds * 1000 : Timeout.Infinite
            );
        }

        private void ServiceCallback(object state)
        {
            try
            {
                if (!_settings.IsOn)
                {
                    return;
                }

                Logger.LogInformation($"Выполнение задачи по таймеру");
                var sendToSleep = false;
                if (!SleepService.WasLastWakeByPowerButton(_settings.WakeUpSourceExclusions))
                {
                    _settings.LastSleepSend = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    sendToSleep = true;
                }

                var appName = string.IsNullOrWhiteSpace(_version) ? AppName : $"{AppName} ({_version})";
                var text =
                    $"{appName} (Интервал: {_settings.GetTimeInterval()})\nПоследний запуск: {DateTime.Now:dd.MM.yyyy HH:mm}";
                if (!string.IsNullOrWhiteSpace(_settings.LastSleepSend))
                {
                    text += $"\nПоследнее отправление в сон: {_settings.LastSleepSend}";
                }

                try
                {
                    if (!IsDisposed)
                    {
                        Invoke((MethodInvoker)delegate { _trayIcon.Text = text; });
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Invoke UI failed: " + ex);
                }

                if (sendToSleep)
                {
                    SleepService.Sleep();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка при выполнении задачи по таймеру: {ex.Message}");
            }
        }

        private void OnToggleClick(object sender, EventArgs e)
        {
            _settings.IsOn = !_settings.IsOn;
            UpdateState();
        }

        private void UpdateState()
        {
            _trayIcon.Icon = new Icon(_settings.IsOn ? "Assets\\icon_on.ico" : "Assets\\icon_off.ico");
            _toggleItem.Text = _settings.IsOn ? "Выключить" : "Включить";

            if (_settings.IsOn)
            {
                _serviceTimer.Change(0, _settings.IntervalSeconds * 1000);
            }
            else
            {
                _serviceTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            _serviceTimer?.Dispose();
            _trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serviceTimer?.Dispose();
                _trayIcon.Dispose();
                _pipeServer?.Dispose();
            }

            base.Dispose(disposing);
        }

        private string GetVersion()
        {
            var versionFileName = "version.json";
            if (!File.Exists(versionFileName))
            {
                return string.Empty;
            }

            var text = File.ReadAllText(versionFileName);
            var info = JsonConvert.DeserializeObject<VersionInfo>(text);
            return info.version;
        }

        private class VersionInfo
        {
            public string version { get; set; }
        }
    }
}