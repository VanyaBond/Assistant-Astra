using Vosk;
using NAudio.Wave;
using System.Diagnostics;
using VolControl;
using Sound;
using Pv;

class Program {
    static bool _isListening;

    static void Main() {
        try {
            string modelPath = @"D:\Projects\Pulse\voskSharpLP\vosk-model-small-ru-0.22";
            const string accessKey = "api";

            var keywordPaths = new List<string> {
                @"D:\Projects\Pulse\astraAct\Astra_en_windows_v3_0_0.ppn"
            };

            // Загрузка моделі vosk
            Model model = new Model(modelPath);
            VoskRecognizer recognizer = new VoskRecognizer(model, 16000);

            using (Porcupine handle = Porcupine.FromKeywordPaths(accessKey, keywordPaths)) {
                using (var waveIn = new WaveInEvent()) {
                    waveIn.DeviceNumber = 0;
                    waveIn.WaveFormat = new WaveFormat(16000, 1);
                    waveIn.BufferMilliseconds = (int)((handle.FrameLength / (double)handle.SampleRate) * 1000);

                    // Один буфер для фреймів
                    short[] audioFrame = new short[handle.FrameLength];

                    waveIn.DataAvailable += (sender, e) => {
                        // Копіюємо тільки необхідні дані в один і той же буфер
                        Buffer.BlockCopy(e.Buffer, 0, audioFrame, 0, handle.FrameLength * 2);

                        // Перша обробка
                        var keywordIndex = handle.Process(audioFrame);

                        // пошук ключ слова
                        if (keywordIndex >= 0 && !_isListening) {
                            ProcessCom.SayHi();
                            _isListening = true; // вкл прослушку
                        }

                        // обробка команд
                        if (_isListening && recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded)) {
                            string result = recognizer.Result().ToLower();
                            Console.WriteLine(result);

                            // якщо є команда
                            ProcessCommand(result);
                            _isListening = false; // НЕ ЗАБУТИ ПРОДЛИТИ ПРОСЛУШКУ НА 10 СЕК
                        }
                    };

                    waveIn.StartRecording();
                    Console.WriteLine("Голосовий асистент 'Астра' запущено.");
                    ProcessCom.StartHi();
                    Console.ReadLine();
                    waveIn.StopRecording();
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine("Помилка: " + ex.Message);
        }
    }
    
    static void ProcessCommand(string text) {
        // все про браузер
        if (text.Contains("открой браузер") || text.Contains("запусти браузер") || text.Contains("гугл") ||
            text.Contains("хром")) {
            ProcessCom.ConfS();
            Process.Start("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe", "chrome://newtab");
        }
        else if (text.Contains("открой ютуб") || text.Contains("включи ютуб") || text.Contains("включи ютюб")) {
            ProcessCom.ConfS();
            Process.Start("explorer", "https://www.youtube.com/");
        }
        else if (text.Contains("включи музыку") || text.Contains("музыку")) {
            ProcessCom.ConfS();
            Process.Start("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
                "https://music.youtube.com/playlist?list=LM");
        }

        // стимчик
        else if (text.Contains("открой с тим") || text.Contains("включи с тим") || text.Contains("запусти с тим")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\steam\\ahk\\openSteam.exe");
            ProcessCom.ConfS();
        }
        else if (text.Contains("закрой с тим") || text.Contains("выключи с тим") || text.Contains("отключи с тим")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\steam\\ahk\\closeSteam.exe");
            ProcessCom.ConfS();
        }

        // все про звук
        else if (text.Contains("сделай погромче") || text.Contains("сделай громкость побольше") ||
                 text.Contains("добавь громкость") || text.Contains("ещё громче") ||
                 text.Contains("погромче") || text.Contains("громче")) {
            VolumeControl.AddVol();
            ProcessCom.ConfS();
        }
        else if (text.Contains("сделай потише") || text.Contains("сделай тише") ||
                 text.Contains("ещё потише") || text.Contains("сделать тише") ||
                 text.Contains("потише") || text.Contains("тише")) {
            VolumeControl.MinVol();
            ProcessCom.ConfS();
        }
        else if (text.Contains("тихо") || text.Contains("выключи звук") ||
                 text.Contains("мут") || text.Contains("молчи")) {
            VolumeControl.MuteVolume();
            ProcessCom.ConfS();
        }
        else if (text.Contains("можешь говорить") || text.Contains("включи звук") ||
                 text.Contains("ан мут")) {
            VolumeControl.UnMuteVolume();
            ProcessCom.ConfS();
        }

        // прочее
        else if (text.Contains("молодец") || text.Contains("спасибо") ||
                 text.Contains("благодарю")) {
            ProcessCom.ThX();
        }
        else if (text.Contains("смени язык") || text.Contains("переключи язык") ||
                 text.Contains("переключи мову") || text.Contains("смени раскладку")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\windows\\ahk\\setLang.exe");
            ProcessCom.ConfS();
        }
        else if (text.Contains("повысить производительность системы") || text.Contains("ускорь систему") ||
                 text.Contains("игровой режим") || text.Contains("производительность на максимум")) {
            // ProcessCom.ConfS();
            // Thread.Sleep(500);
            ProcessCom.BsT();
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\msi\\ahk\\openBoost.exe");
        }
        else if (text.Contains("очисти корзину") || text.Contains("почисти корзину") ||
                 text.Contains("удали мусор") || text.Contains("почисти мусор")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\windows\\ahk\\emptyTrash.exe");
            ProcessCom.ConfS();
        }
        else if (text.Contains("режим сон") || text.Contains("спать") ||
                 text.Contains("режим сна")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\windows\\ahk\\sleep.exe");
            ProcessCom.ConfS();
        }
        /*
        else if (text.Contains("перезагрузись") || text.Contains("перезагрузка") ||
                 text.Contains("перезапустись")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\windows\\ahk\\screenshot.exe");
            ProcessCom.ConfS();
        }
        */

        else if (text.Contains("сделай скриншот") || text.Contains("скрин") ||
                 text.Contains("скриншот") || text.Contains("сделай скрин")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\windows\\ahk\\screenshot.exe");
            ProcessCom.ConfS();
        }
        else if (text.Contains("покажи буфер") || text.Contains("открой буфер") ||
                 text.Contains("буфер обмена") || text.Contains("буфер")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\windows\\ahk\\clipboard.exe");
            ProcessCom.ConfS();
        }
        else if (text.Contains("сверни все окна") || text.Contains("на рабочий стол") ||
                 text.Contains("сверни окна") || text.Contains("руки на стол")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\windows\\ahk\\rollWindows.exe");
            ProcessCom.ConfS();
        }
        else if (text.Contains("открой диспетчер") || text.Contains("диспетчер задач") ||
                 text.Contains("открой диспетчер задач") || text.Contains("открой таско менеджер")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\windows\\ahk\\taskManager.exe");
            ProcessCom.ConfS();
        }
        else if (text.Contains("блокировка") || text.Contains("заблокируйте") ||
                 text.Contains("систем блок") || text.Contains("заблокируй ся")) {
            Process.Start("D:\\Projects\\Pulse\\astraCommands\\windows\\ahk\\blocking.exe");
            ProcessCom.ConfS();
        }
        else if (text.Contains("инфо")) {
            ProcessCom.InF();
        }

        // стопер
        else if (text.Contains("стой") || text.Contains("стоп") || text.Contains("выключись") ||
                 text.Contains("отключись")) {
            ProcessCom.Pof();
            Environment.Exit(0);
        }
        else {
            ProcessCom.WhT();
        }
    }
}