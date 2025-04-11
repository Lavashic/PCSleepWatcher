using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices;

namespace PCSleepWatcher
{
    public static class SleepService
    {
        public static void Sleep()
        {
            try
            {
                Logger.LogInformation("Отправление компьютера в сон");
                var success = SetSuspendState(false, false, false);
                if (success)
                {
                    return;
                }

                var errorCode = Marshal.GetLastWin32Error();
                var errorWarn = $"Не удалось отправить компьютер в сон.\nКод ошибки: {errorCode}";
                Logger.LogWarning(errorWarn);
                MessageBox.Show(errorWarn, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                var error = $"Не удалось отправить компьютер в сон.\nОшибка: {ex.Message}";
                Logger.LogWarning(error);
                MessageBox.Show(error, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool WasLastWakeByPowerButton(string[] wakeUpSourceExclusions)
        {
            try
            {
                Logger.LogInformation($"Получение последнего источника пробуждения");
                var query = "*[System[Provider[@Name='Microsoft-Windows-Power-Troubleshooter'] and (EventID=1)]]";
                var logQuery = new EventLogQuery("System", PathType.LogName, query)
                {
                    ReverseDirection = true
                };
                var logReader = new EventLogReader(logQuery);

                var lastEvent = logReader.ReadEvent();
                if (lastEvent == null)
                {
                    return false;
                }

                var description = lastEvent.FormatDescription().ToLowerInvariant();
                var wakeSourceText = "wake source";
                var wakeSourceTextRu = "источник выхода";
                if (description.Contains(wakeSourceTextRu))
                {
                    wakeSourceText = wakeSourceTextRu;
                }

                foreach (var item in wakeUpSourceExclusions)
                {
                    if (description.EndsWith($"{wakeSourceText}: {item}"))
                    {
                        Logger.LogInformation($"Последний источник пробуждения: {item}");
                        return true;
                    }
                }

                Logger.LogInformation($"Последний источник пробуждения не среди исключений: {description}");
                return false;

            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка при получении последнего источника пробуждения: {ex.Message}");
            }

            return false;
        }

        [DllImport("powrprof.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);
    }
}