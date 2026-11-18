using System;
using System.Windows.Controls;
using System.Windows.Threading;
using MediaHub.Core.Models;

namespace MediaHub.App;

// Ховає роботу з MediaElement: решта програми не знає, як саме відтворюється файл
public class PlayerService
{
    private readonly MediaElement _element;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(500) };

    // Подія для оновлення смуги прогресу
    public event Action? PositionChanged;

    public PlayerService(MediaElement element)
    {
        _element = element;
        _element.LoadedBehavior = MediaState.Manual;
        _element.UnloadedBehavior = MediaState.Manual;
        _timer.Tick += (_, _) => PositionChanged?.Invoke();
    }

    public TimeSpan Position => _element.Position;

    public TimeSpan Duration =>
        _element.NaturalDuration.HasTimeSpan ? _element.NaturalDuration.TimeSpan : TimeSpan.Zero;

    public double Volume
    {
        get => _element.Volume;
        set => _element.Volume = value;
    }

    public void Play(MediaFile file)
    {
        _element.Source = new Uri(file.FilePath);
        _element.Play();
        _timer.Start();
    }

    public void Resume()
    {
        _element.Play();
        _timer.Start();
    }

    public void Pause()
    {
        _element.Pause();
        _timer.Stop();
    }

    public void Stop()
    {
        _element.Stop();
        _timer.Stop();
        PositionChanged?.Invoke();
    }
}