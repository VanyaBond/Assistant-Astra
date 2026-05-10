using Vosk;
using NAudio.Wave;
using System.Diagnostics;
using VolControl; 
using Sound;     
using Pv;         
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Program
{

    static bool _isListening;
    static System.Timers.Timer _listeningTimer;
    

    static readonly string AppPath = AppDomain.CurrentDomain.BaseDirectory;

    static readonly Dictionary<string[], Action> Commands = new Dictionary<string[], Action>
    {
        // --- Браузер та веб ---
        { new[] { "браузер", "гугл", "хром" }, () => RunApp("chrome.exe", "chrome://newtab") },
        { new[] { "ютуб", "ютюб" }, () => RunApp("explorer", "https://www.youtube.com/") },
        { new[] { "музыку", "музыка" }, () => RunApp("chrome.exe", "https://music.youtube.com/playlist?list=LM") },

        // --- Steam та ігри (через AHK скрипти) ---
        { new[] { "открой с тим", "включи с тим", "запусти с тим" }, () => RunTool(@"steam\ahk\openSteam.exe") },
        { new[] { "закрой с тим", "выключи с тим" }, () => RunTool(@"steam\ahk\closeSteam.exe") },

        // --- Керування звуком (Issue #3: SRP) ---
        { new[] { "громче", "погромче", "добавь громкость" }, () => { VolumeControl.AddVol(); ProcessCom.ConfS(); } },
        { new[] { "тише", "потише", "сделай тише" }, () => { VolumeControl.MinVol(); ProcessCom.ConfS(); } },
        { new[] { "мут", "выключи звук", "тихо" }, () => { VolumeControl.MuteVolume(); ProcessCom.ConfS(); } },
        { new[] { "говорить", "ан мут" }, () => { VolumeControl.UnMuteVolume(); ProcessCom.ConfS(); } },

        // --- Системні утиліти ---
        { new[] { "смени язык", "смени раскладку", "переключи язык" }, () => RunTool(@"windows\ahk\setLang.exe") },
        { new[] { "скриншот", "скрин", "сделай скрин" }, () => RunTool(@"windows\ahk\screenshot.exe") },
        { new[] { "корзину", "мусор" }, () => RunTool(@"windows\ahk\emptyTrash.exe") },
        { new[] { "диспетчер задач", "таск менеджер" }, () => RunTool(@"windows\ahk\taskManager.exe") },
        { new[] { "сверни окна", "рабочий стол" }, () => RunTool(@"windows\ahk\rollWindows.exe") },
        { new[] { "сон", "спать" }, () => RunTool(@"windows\ahk\sleep.exe") },

        // --- Інше ---
        { new[] { "инфо", "информация" }, () => ProcessCom.InF() },
        { new[] { "спасибо", "молодец" }, () => ProcessCom.ThX() },
        
        // --- Вимкнення програми ---
        { new[] { "стой", "стоп", "выключись", "отключись" }, () => { 
            ProcessCom.Pof(); 
            Thread.Sleep(800); 
            Environment.Exit(0); 
        } }
    };

    static void Main(string[] args)
    {

        _listeningTimer = new System.Timers.Timer(10000);
        _listeningTimer.Elapsed += (s, e) => {
            if (_isListening) {
                _isListening = false;
                Console.WriteLine("--- Тайм-аут: перехід у режим очікування ключового слова ---");
            }
        };
        _listeningTimer.AutoReset = false;

        try
        {
            // Динамічні шляхи (Issue #1)
            string modelPath = Path.Combine(AppPath, "Models", "vosk-model-small-ru-0.22");
            string ppnPath = Path.Combine(AppPath, "Resources", "Astra_en_windows_v3_0_0.ppn");
            const string accessKey = "ТВІЙ_PICOVOICE_ACCESS_KEY"; 


            Model model = new Model(modelPath);
            VoskRecognizer recognizer = new VoskRecognizer(model, 16000);

            using (Porcupine handle = Porcupine.FromKeywordPaths(accessKey, new List<string> { ppnPath }))
            using (var waveIn = new WaveInEvent())
            {
                waveIn.DeviceNumber = 0;
                waveIn.WaveFormat = new WaveFormat(16000, 1);
                waveIn.BufferMilliseconds = (int)((handle.FrameLength / (double)handle.SampleRate) * 1000);

                short[] audioFrame = new short[handle.FrameLength];

                waveIn.DataAvailable += (sender, e) =>
                {
                    Buffer.BlockCopy(e.Buffer, 0, audioFrame, 0, handle.FrameLength * 2);
                    
                    if (handle.Process(audioFrame) >= 0 && !_isListening)
                    {
                        Console.WriteLine("[!] Астра активована. Слухаю...");
                        ProcessCom.SayHi();
                        _isListening = true;
                        _listeningTimer.Start(); 
                    }

                    if (_isListening && recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
                    {
                        string result = ExtractTextFromJson(recognizer.Result()).ToLower();
                        
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            Console.WriteLine($">> Ви сказали: {result}");
                            ProcessCommand(result);
                            
                            _isListening = false;
                            _listeningTimer.Stop();
                        }
                    }
                };

                waveIn.StartRecording();
                Console.WriteLine("=== Ассистент Астра запущена ===");
                ProcessCom.StartHi();
                
                Console.WriteLine("Натисніть Enter для завершення...");
                Console.ReadLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критична помилка: {ex.Message}");
            Console.ReadKey();
        }
    }

    static void ProcessCommand(string text)
    {
        bool isExecuted = false;

        foreach (var commandEntry in Commands)
        {
 
            if (commandEntry.Key.Any(k => text.Contains(k)))
            {
                commandEntry.Value.Invoke();
                isExecuted = true;
                break;
            }
        }

        if (!isExecuted)
        {
            Console.WriteLine("Команда не розпізнана.");
            ProcessCom.WhT();
        }
    }


    static void RunApp(string fileName, string args = "")
    {
        try {
            Process.Start(new ProcessStartInfo(fileName, args) { UseShellExecute = true });
            ProcessCom.ConfS();
        } catch { Console.WriteLine($"Не вдалося запустити: {fileName}"); }
    }

    static void RunTool(string relativePath)
    {
        string fullPath = Path.Combine(AppPath, "Commands", relativePath);
        if (File.Exists(fullPath)) {
            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
            ProcessCom.ConfS();
        } else {
            Console.WriteLine($"Файл не знайдено: {fullPath}");
        }
    }

    static string ExtractTextFromJson(string json)
    {

        int start = json.IndexOf("\"text\" : \"") + 10;
        int end = json.LastIndexOf("\"");
        return (start > 9 && end > start) ? json.Substring(start, end - start) : "";
    }
}
