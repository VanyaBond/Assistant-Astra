namespace VolControl;

using NAudio.CoreAudioApi;

public static class VolumeControl {
    private static MMDevice _device =
        new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

    public static void AddVol(float step = 0.1f) {
        float newVolume = Math.Min(_device.AudioEndpointVolume.MasterVolumeLevelScalar + step, 1.0f);
        _device.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume;
        Console.WriteLine($"Громкость увеличена до {newVolume * 100}%");
    }

    public static void MinVol(float step = 0.1f) {
        float newVolume = Math.Max(_device.AudioEndpointVolume.MasterVolumeLevelScalar - step, 0.0f);
        _device.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume;
        Console.WriteLine($"Громкость уменьшена до {newVolume * 100}%");
    }

    public static void MuteVolume() {
        var mute = new MMDeviceEnumerator();
        var defDevice = mute.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        defDevice.AudioEndpointVolume.Mute = true;
    }

    public static void UnMuteVolume() {
        var unMute = new MMDeviceEnumerator();
        var defDevice = unMute.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        defDevice.AudioEndpointVolume.Mute = false;
    }

    /*
    public static void SetVolume(float level) {
        level = Math.Clamp(level, 0.0f, 1.0f);
        device.AudioEndpointVolume.MasterVolumeLevelScalar = level;
        Console.WriteLine($"Громкость установлена на {level * 100}%");
    } */
}