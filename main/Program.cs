using Vosk;
using NAudio.Wave;
using System.Diagnostics;
using VolControl;
using Sound;
using Pv;
using System.IO;
using System.Timers;

class Program
{
    static bool _isListening;
    static System.Timers.Timer _listeningTimer;
    

    static readonly string AppPath = AppDomain.CurrentDomain.BaseDirectory;

    static void Main()
    {
        _listeningTimer = new System.Timers.Timer(10000);
        _listeningTimer.Elapsed += (s, e) => {
            _isListening = false;
            Console.WriteLine("--- Режим очікування (тайм-аут) ---");
        };
        _listeningTimer.AutoReset = false;

        try
        {

            string modelPath = Path.Combine(AppPath, "Models", "vosk-model-small-ru-0.22");
            string ppnPath = Path.Combine(AppPath, "Resources", "Astra_en_windows_v3_0_0.ppn");

            const string accessKey = "YOUR_PICOVOICE_API_KEY"; 

            // Перевірка наявності критичних файлів перед стартом
            if (!Directory.Exists(modelPath)) throw new DirectoryNotFoundException($"Модель не знайдено: {modelPath}");
            if (!File.Exists(ppnPath)) throw new FileNotFoundException($"Файл активації не знайдено: {ppnPath}");

            // Ініціалізація Vosk
            Model model = new Model(modelPath);
            VoskRecognizer recognizer = new VoskRecognizer(model, 16000);

            // Ініціалізація Porcupine
            using (Porcupine handle = Porcupine.FromKeywordPaths(accessKey, new List<string> { ppnPath }))
            {
                using (var waveIn = new WaveInEvent())
                {
                    waveIn.DeviceNumber = 0;
                    waveIn.WaveFormat = new WaveFormat(16000, 1);
                    waveIn.BufferMilliseconds = (int)((handle.FrameLength / (double)handle.SampleRate) * 1000);

                    short[] audioFrame = new short[handle.FrameLength];

                    waveIn.DataAvailable += (sender, e) =>
                    {

                        Buffer.BlockCopy(e.Buffer, 0, audioFrame, 0, handle.FrameLength * 2);
                        var keywordIndex = handle.Process(audioFrame);

                        if (keywordIndex >= 0 && !_isListening)
                        {
                            Console.WriteLine("--- Слухаю команду... ---");
                            ProcessCom.SayHi();
                            _isListening = true;
                            _listeningTimer.Start(); // Запускаємо відлік 10 секунд
                        }

                        if (_isListening && recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
                        {
                            string resultRaw = recognizer.Result();

                            string text = ExtractTextFromJson(resultRaw).ToLower();

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                Console.WriteLine($"Команда: {text}");
                                ProcessCommand(text);
                                
                                _isListening = false; 
                                _listeningTimer.Stop(); // Скидаємо таймер після виконання
                            }
                        }
                    };

                    waveIn.StartRecording();
                    Console.WriteLine("Голосовий асистент 'Астра' готовий до роботи.");
                    ProcessCom.StartHi();
                    
                    Console.WriteLine("Натисніть Enter для виходу...");
                    Console.ReadLine();
                    
                    waveIn.StopRecording();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критична помилка: {ex.Message}");
        }
    }

    static void ProcessCommand(string text)
    {

        void RunTool(string relativePath)
        {
            string fullPath = Path.Combine(AppPath, "Commands", relativePath);
            if (File.Exists(fullPath))
            {
                Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
                ProcessCom.ConfS();
            }
            else
            {
                Console.WriteLine($"Помилка: Файл не знайдено за шляхом {fullPath}");
            }
        }

        // --- ЛОГІКА КОМАНД ---

        // Браузер та інтернет
        if (ContainsAny(text, "открой браузер", "запусти браузер", "гугл", "хром"))
        {
            ProcessCom.ConfS();
            Process.Start(new ProcessStartInfo("chrome.exe", "chrome://newtab") { UseShellExecute = true });
        }
        else if (ContainsAny(text, "ютуб", "ютюб"))
        {
            ProcessCom.ConfS();
            Process.Start(new ProcessStartInfo("explorer", "https://www.youtube.com/") { UseShellExecute = true });
        }

        // Стім (через AHK)
        else if (ContainsAny(text, "открой с тим", "включи с тим", "запусти с тим"))
        {
            RunTool(@"steam\ahk\openSteam.exe");
        }
        else if (ContainsAny(text, "закрой с тим", "выключи с тим"))
        {
            RunTool(@"steam\ahk\closeSteam.exe");
        }

        // Звук
        else if (ContainsAny(text, "громче", "погромче", "добавь громкость"))
        {
            VolumeControl.AddVol();
            ProcessCom.ConfS();
        }
        else if (ContainsAny(text, "тише", "потише", "сделай тише"))
        {
            VolumeControl.MinVol();
            ProcessCom.ConfS();
        }
        else if (ContainsAny(text, "мут", "выключи звук", "тихо"))
        {
            VolumeControl.MuteVolume();
            ProcessCom.ConfS();
        }

        else if (ContainsAny(text, "смени язык", "смени раскладку"))
        {
            RunTool(@"windows\ahk\setLang.exe");
        }
        else if (ContainsAny(text, "скриншот", "сделай скрин"))
        {
            RunTool(@"windows\ahk\screenshot.exe");
        }
        else if (ContainsAny(text, "очисти корзину", "удали мусор"))
        {
            RunTool(@"windows\ahk\emptyTrash.exe");
        }
        else if (ContainsAny(text, "диспетчер задач"))
        {
            RunTool(@"windows\ahk\taskManager.exe");
        }

        else if (ContainsAny(text, "стой", "стоп", "выключись"))
        {
            ProcessCom.Pof();
            Thread.Sleep(500);
            Environment.Exit(0);
        }
        else if (ContainsAny(text, "инфо"))
        {
            ProcessCom.InF();
        }
        else
        {
            ProcessCom.WhT(); 
        }
    }

    static bool ContainsAny(string text, params string[] keywords)
    {
        foreach (var key in keywords)
        {
            if (text.Contains(key)) return true;
        }
        return false;
    }

    static string ExtractTextFromJson(string json)
    {

        int start = json.IndexOf("\"text\" : \"") + 10;
        int end = json.LastIndexOf("\"");
        if (start > 9 && end > start)
            return json.Substring(start, end - start);
        return "";
    }
}﻿
