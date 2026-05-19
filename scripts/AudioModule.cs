using Godot;
using System.Collections.Generic;

/// <summary>
/// AudioManager — Autoload singleton.
/// Add this script to a Node named "AudioManager" in Project > Project Settings > Globals > Autoload.
///
/// Required AudioServer buses (set up in the Audio tab at the bottom of the Godot editor):
///   Master  (index 0, created automatically)
///   Music   (index 1)
///   SFX     (index 2)
/// </summary>
/// // Add this inside the AudioManager class

public partial class AudioModule : Node
{
    public static AudioModule Instance { get; private set; }

    // ──────────────────────────────────────────────────────────────
    // Constants
    // ──────────────────────────────────────────────────────────────
    private const string MusicBus   = "Music";
    private const string SfxBus     = "SFX";
    private const int    SfxPoolSize = 8;            // simultaneous SFX voices
    private const float  FadeDuration = 1.0f;        // music crossfade seconds

    // ──────────────────────────────────────────────────────────────
    // State
    // ──────────────────────────────────────────────────────────────
    private AudioStreamPlayer[]   _sfxPool;
    private int                   _sfxPoolIndex = 0;

    private AudioStreamPlayer     _musicPlayerA;
    private AudioStreamPlayer     _musicPlayerB;
    private bool                  _musicActiveA = true;

    private Tween                 _musicTween;

    // Volume (linear 0..1) per bus, persisted across scenes
    private readonly Dictionary<string, float> _volumes = new()
    {
        { "Master", 1.0f },
        { MusicBus, 1.0f },
        { SfxBus,   1.0f },
    };

    private readonly Dictionary<string, bool> _muted = new()
    {
        { "Master", false },
        { MusicBus, false },
        { SfxBus,   false },
    };

    // Optional: cache loaded streams to avoid repeated disk I/O
    private readonly Dictionary<string, AudioStream> _streamCache = new();

    // ──────────────────────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────────────────────
    public override void _Ready()
    {
        Instance = this;
        
        // Build SFX pool
        _sfxPool = new AudioStreamPlayer[SfxPoolSize];
        for (int i = 0; i < SfxPoolSize; i++)
        {
            var player = new AudioStreamPlayer();
            player.Bus = SfxBus;
            AddChild(player);
            _sfxPool[i] = player;
        }

        // Music players (A/B for crossfading)
        _musicPlayerA = new AudioStreamPlayer { Bus = MusicBus, VolumeDb = 0f };
        _musicPlayerB = new AudioStreamPlayer { Bus = MusicBus, VolumeDb = -80f };
        AddChild(_musicPlayerA);
        AddChild(_musicPlayerB);

        // Restore saved volumes (e.g. from a config file)
        ApplyAllVolumes();
    }

    // ──────────────────────────────────────────────────────────────
    // SFX API
    // ──────────────────────────────────────────────────────────────

    /// <summary>Play a sound effect by resource path.</summary>
    public void PlaySFX(string path, float volumeDb = 0f, float pitchScale = 1f)
    {
        var stream = LoadStream(path);
        if (stream == null) return;

        var player = NextSfxPlayer();
        player.Stream     = stream;
        player.VolumeDb   = volumeDb;
        player.PitchScale = pitchScale;
        player.Play();
    }

    /// <summary>Play a sound effect from a pre-loaded AudioStream reference.</summary>
    public void PlaySFXStream(AudioStream stream, float volumeDb = 0f, float pitchScale = 1f)
    {
        if (stream == null) return;

        var player = NextSfxPlayer();
        player.Stream     = stream;
        player.VolumeDb   = volumeDb;
        player.PitchScale = pitchScale;
        player.Play();
    }

    /// <summary>Stop all currently playing SFX.</summary>
    public void StopAllSFX()
    {
        foreach (var p in _sfxPool)
            p.Stop();
    }

    // ──────────────────────────────────────────────────────────────
    // Music API
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Play music with an optional crossfade.
    /// If the same track is already playing, this call is ignored.
    /// </summary>
    public void PlayMusic(string path, bool loop = true, bool crossfade = true)
    {
        var stream = LoadStream(path);
        if (stream == null) return;

        // Same track already playing?
        var current = _musicActiveA ? _musicPlayerA : _musicPlayerB;
        if (current.Playing && current.Stream == stream) return;

        SetStreamLoop(stream, loop);

        var incoming = _musicActiveA ? _musicPlayerB : _musicPlayerA;
        var outgoing = _musicActiveA ? _musicPlayerA : _musicPlayerB;

        incoming.Stream   = stream;
        incoming.VolumeDb = crossfade ? -80f : 0f;
        incoming.Play();

        if (crossfade)
            CrossfadeTo(incoming, outgoing);
        else
            outgoing.Stop();

        _musicActiveA = !_musicActiveA;
    }

    /// <summary>Stop music, optionally fading out.</summary>
    public void StopMusic(bool fade = true)
    {
        var player = _musicActiveA ? _musicPlayerA : _musicPlayerB;
        if (!player.Playing) return;

        if (fade)
        {
            _musicTween?.Kill();
            _musicTween = CreateTween();
            _musicTween.TweenProperty(player, "volume_db", -80f, FadeDuration)
                       .SetTrans(Tween.TransitionType.Sine);
            _musicTween.TweenCallback(Callable.From(player.Stop));
        }
        else
        {
            player.Stop();
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Volume / Mute API
    // ──────────────────────────────────────────────────────────────

    /// <summary>Set volume (0.0 – 1.0 linear) for a named bus.</summary>
    public void SetVolume(string busName, float linear)
    {
        linear = Mathf.Clamp(linear, 0f, 1f);
        _volumes[busName] = linear;
        ApplyVolume(busName);
    }

    /// <summary>Get current linear volume (0..1) for a bus.</summary>
    public float GetVolume(string busName)
        => _volumes.GetValueOrDefault(busName, 1f);

    /// <summary>Mute or unmute a bus.</summary>
    public void SetMute(string busName, bool mute)
    {
        _muted[busName] = mute;
        int idx = AudioServer.GetBusIndex(busName);
        if (idx >= 0)
            AudioServer.SetBusMute(idx, mute);
    }

    public bool IsMuted(string busName)
        => _muted.GetValueOrDefault(busName, false);

    public void ToggleMute(string busName)
        => SetMute(busName, !IsMuted(busName));

    // ──────────────────────────────────────────────────────────────
    // Cache management
    // ──────────────────────────────────────────────────────────────

    /// <summary>Pre-load a stream into the cache (call from a loading screen).</summary>
    public void Preload(string path)
        => LoadStream(path);

    /// <summary>Remove a stream from the cache to free memory.</summary>
    public void Unload(string path)
        => _streamCache.Remove(path);

    public void ClearCache()
        => _streamCache.Clear();

    // ──────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────

    private AudioStreamPlayer NextSfxPlayer()
    {
        // Round-robin through the pool; skip any player that was just started
        // this frame to avoid double-playing (simple voice stealer).
        for (int i = 0; i < SfxPoolSize; i++)
        {
            int idx = (_sfxPoolIndex + i) % SfxPoolSize;
            if (!_sfxPool[idx].Playing)
            {
                _sfxPoolIndex = (idx + 1) % SfxPoolSize;
                return _sfxPool[idx];
            }
        }
        // All voices busy — steal the oldest one (current index)
        var stolen = _sfxPool[_sfxPoolIndex];
        _sfxPoolIndex = (_sfxPoolIndex + 1) % SfxPoolSize;
        return stolen;
    }

    private AudioStream LoadStream(string path)
    {
        if (_streamCache.TryGetValue(path, out var cached))
            return cached;

        if (!ResourceLoader.Exists(path))
        {
            GD.PrintErr($"[AudioManager] Resource not found: {path}");
            return null;
        }

        var stream = ResourceLoader.Load<AudioStream>(path);
        if (stream == null)
        {
            GD.PrintErr($"[AudioManager] Failed to load AudioStream: {path}");
            return null;
        }

        _streamCache[path] = stream;
        return stream;
    }

    private void CrossfadeTo(AudioStreamPlayer incoming, AudioStreamPlayer outgoing)
    {
        _musicTween?.Kill();
        _musicTween = CreateTween().SetParallel();
        _musicTween.TweenProperty(incoming, "volume_db", 0f, FadeDuration)
                   .SetTrans(Tween.TransitionType.Sine);
        _musicTween.TweenProperty(outgoing, "volume_db", -80f, FadeDuration)
                   .SetTrans(Tween.TransitionType.Sine);
        // Stop the outgoing player after the fade
        var stopTween = CreateTween();
        stopTween.TweenInterval(FadeDuration);
        stopTween.TweenCallback(Callable.From(outgoing.Stop));
    }

    private static void SetStreamLoop(AudioStream stream, bool loop)
    {
        // OGG
        if (stream is AudioStreamOggVorbis ogg)
            ogg.Loop = loop;
        // WAV
        else if (stream is AudioStreamWav wav)
            wav.LoopMode = loop
                ? AudioStreamWav.LoopModeEnum.Forward
                : AudioStreamWav.LoopModeEnum.Disabled;
        // MP3
        else if (stream is AudioStreamMP3 mp3)
            mp3.Loop = loop;
    }

    private void ApplyVolume(string busName)
    {
        int idx = AudioServer.GetBusIndex(busName);
        if (idx < 0)
        {
            GD.PrintErr($"[AudioManager] Bus not found: {busName}");
            return;
        }
        float linear = _volumes.GetValueOrDefault(busName, 1f);
        AudioServer.SetBusVolumeDb(idx, Mathf.LinearToDb(linear));
    }

    private void ApplyAllVolumes()
    {
        foreach (var busName in _volumes.Keys)
            ApplyVolume(busName);
        foreach (var (busName, muted) in _muted)
        {
            int idx = AudioServer.GetBusIndex(busName);
            if (idx >= 0)
                AudioServer.SetBusMute(idx, muted);
        }
    }
}