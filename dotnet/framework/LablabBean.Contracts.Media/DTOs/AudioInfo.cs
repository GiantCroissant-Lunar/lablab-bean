namespace LablabBean.Contracts.Media.DTOs;

/// <summary>
/// Audio stream metadata
/// </summary>
/// <param name="SampleRate">Sample rate in Hz (e.g., 44100, 48000)</param>
/// <param name="Channels">Number of audio channels (1=mono, 2=stereo)</param>
/// <param name="Codec">Audio codec name (e.g., "aac", "mp3", "opus")</param>
/// <param name="BitRate">Average bit rate in bits/second (0 if unknown)</param>
public record AudioInfo(
    int SampleRate,
    int Channels,
    string Codec,
    long BitRate
);
