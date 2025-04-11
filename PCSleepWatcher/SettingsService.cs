using Newtonsoft.Json;

namespace PCSleepWatcher
{
    public class SettingsService
    {
        private const string StateFilePath = "settings.json";
        private int _intervalSeconds;
        private bool _isOn;
        private string _lastSleepSend;

        public int IntervalSeconds
        {
            get => _intervalSeconds;
            set
            {
                if (value == _intervalSeconds)
                {
                    return;
                }

                _intervalSeconds = value;
                SaveState();
            }
        }

        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (value == _isOn)
                {
                    return;
                }

                _isOn = value;
                SaveState();
            }
        }

        public string LastSleepSend
        {
            get => _lastSleepSend;
            set
            {
                if (value == _lastSleepSend)
                {
                    return;
                }

                _lastSleepSend = value;
                SaveState();
            }
        }

        public string[] WakeUpSourceExclusions { get; private set; }

        private void SaveState()
        {
            try
            {
                Logger.LogInformation($"Сохранение настроек");
                var state = new AppState
                {
                    IsOn = IsOn, IntervalSeconds = IntervalSeconds, LastSleepSend = LastSleepSend,
                    WakeUpSourceExclusions = WakeUpSourceExclusions
                };
                File.WriteAllText(StateFilePath, JsonConvert.SerializeObject(state));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка при сохранении настроек: {ex.Message}");
            }
        }

        public void LoadState()
        {
            try
            {
                Logger.LogInformation($"Загрузка настроек");
                if (!File.Exists(StateFilePath))
                {
                    return;
                }

                var text = File.ReadAllText(StateFilePath);
                var state = JsonConvert.DeserializeObject<AppState>(text);
                _isOn = state?.IsOn ?? false;
                _intervalSeconds = state?.IntervalSeconds ?? 300;
                _lastSleepSend = state?.LastSleepSend ?? string.Empty;
                WakeUpSourceExclusions = state?.WakeUpSourceExclusions ?? [];
            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка при загрузке настроек: {ex.Message}");
            }
        }

        public string GetTimeInterval()
        {
            var seconds = _intervalSeconds;

            if (seconds < 60)
            {
                return $"{seconds} секунд{GetFeminineWordEnding(seconds)}";
            }

            var minutes = seconds / 60;
            seconds = seconds % 60;

            if (minutes < 60)
            {
                var minutesText = $"{minutes} минут{GetFeminineWordEnding(minutes)}";
                return seconds == 0 ? minutesText : $"{minutesText} {seconds} секунд{GetFeminineWordEnding(seconds)}";
            }

            var hours = minutes / 60;
            minutes = minutes % 60;

            var hoursText = $"{hours} час{(hours % 10 >= 5 || hours % 10 == 0 ? "ов" : hours % 10 > 1 ? "а" : "")}";
            return minutes switch
            {
                0 when seconds == 0 => hoursText,
                0 => $"{hoursText} {seconds} секунд{GetFeminineWordEnding(seconds)}",
                _ =>
                    $"{hoursText} {minutes} минут{GetFeminineWordEnding(seconds)} {seconds} секунд{GetFeminineWordEnding(seconds)}"
            };
        }

        private static string GetFeminineWordEnding(int number)
        {
            return number % 10 >= 5 || number % 10 == 0 ? "" : number % 10 > 1 ? "ы" : "а";
        }

        private class AppState
        {
            public bool IsOn { get; init; }
            public int IntervalSeconds { get; init; }
            public string LastSleepSend { get; init; }
            public string[] WakeUpSourceExclusions { get; set; }
        }
    }
}