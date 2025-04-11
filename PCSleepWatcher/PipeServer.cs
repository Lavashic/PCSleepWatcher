using System.IO.Pipes;
using System.Text;
using Timer = System.Threading.Timer;

namespace PCSleepWatcher
{
    public class PipeServer : IDisposable, IAsyncDisposable
    {
        private bool _isStopReceived;
        private bool _isServerCriticalError;
        private bool _isServerRunning;
        private readonly Timer _pipeTimer;
        private readonly SettingsService _settings;

        public PipeServer(SettingsService settings)
        {
            _settings = settings;

            _pipeTimer = new Timer(
                PipeCallback,
                null,
                0,
                60 * 1000
            );
        }

        public EventHandler StateChanged { get; set; }

        private void PipeCallback(object state)
        {
            if (_isServerRunning == false && _isServerCriticalError)
            {
                StartPipeServer();
            }
        }

        public void StartPipeServer()
        {
            var pipeName = "PCSleepWatcherPipe";
            Logger.LogInformation("Старт сервера Pipe");
            while (true)
            {
                try
                {
                    if (_isStopReceived)
                    {
                        break;
                    }

                    _isServerRunning = true;
                    _isServerCriticalError = false;
                    using var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In);
                    Logger.LogInformation("Ожидание подключения клиента внешнего управления...");
                    pipeServer.WaitForConnection();
                    Logger.LogInformation("Клиент внешнего управленияподключён");

                    using var reader = new StreamReader(pipeServer, Encoding.UTF8);
                    var command = reader.ReadLine();
                    Logger.LogInformation($"Получена команда: {command}");

                    switch (command)
                    {
                        case "on":
                            _settings.IsOn = true;
                            StateChanged.Invoke(null, null);
                            break;
                        case "off":
                            _settings.IsOn = false;
                            StateChanged.Invoke(null, null);
                            break;
                    }
                }
                catch (IOException ex)
                {
                    Logger.LogError($"Ошибка Pipe: {ex.Message}. Реинициализация...");
                    _isServerRunning = false;
                    Thread.Sleep(1000);
                }
                catch (ObjectDisposedException ex)
                {
                    Logger.LogError($"Pipe отключился: {ex.Message}. Реинициализация...");
                    _isServerRunning = false;
                    if (!_isStopReceived)
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Неожиданная ошибка Pipe: {ex.Message}");
                    _isServerRunning = false;
                    _isServerCriticalError = true;
                    break;
                }
            }
        }

        private void Stop()
        {
            _isStopReceived = true;
        }

        public void Dispose()
        {
            Logger.LogWarning("Dispose Pipe");
            _pipeTimer?.Dispose();
            Stop();
        }

        public async ValueTask DisposeAsync()
        {
            Logger.LogWarning("Dispose Pipe Async");
            await _pipeTimer.DisposeAsync();
            Stop();
        }
    }
}