namespace Sound;

using NAudio.Wave;

public static class ProcessCom {
    static Random _random = new Random();

    static public void ConfS() {
        PlaySound(RSoundFile());
    }

    static public void SayHi() {
        PlaySound(RSoundFileToSeyHi());
    }

    static public void StartHi() {
        string startHi = "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_start.wav";
        PlaySound(startHi);
    }


    /* старое приветствие
    static public void YSir() {
        string yesSir = "D:\\Projects\\Pulse\\jarvisVoice\\app_sound_jarvis-og_greet1.wav";
        PlaySound(yesSir);
    }
    */

    static public void ThX() {
        string thx = "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_thanks.wav";
        PlaySound(thx);
    }

    static public void WhT() {
        string what = "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_not_found.wav";
        PlaySound(what);
    }

    static public void Pof() {
        string powerOff = "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_off.wav";
        PlaySound(powerOff);
    }

    static public void InF() {
        string info = "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_info.wav";
        PlaySound(info);
    }


    static string RSoundFile() {
        string[] okSir = {
            "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_ok1.wav",
            "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_ok2.wav",
            "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_ok3.wav",
            "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_ok4.wav"
        };
        int index = _random.Next(okSir.Length);
        return okSir[index];
    }

    static string RSoundFileToSeyHi() {
        // Новое приветствие
        string[] sayHi = {
            "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_greet1.wav",
            "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_greet2.wav",
            "D:\\Projects\\Pulse\\jarvisVoice\\jarvis_greet3.wav"
        };
        int index = _random.Next(sayHi.Length);
        return sayHi[index];
    }

    static void PlaySound(string filePath) {
        try {
            using (var audioFile = new AudioFileReader(filePath))
            using (var outputDevice = new WaveOutEvent()) {
                outputDevice.Init(audioFile);
                outputDevice.Play();

                while (outputDevice.PlaybackState == PlaybackState.Playing) {
                    Thread.Sleep(100);
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine("Ошибка воспроизведения звука: " + ex.Message);
        }
    }
}