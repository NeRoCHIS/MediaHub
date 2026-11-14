using MediaHub.Core.Factories;
using MediaHub.Core.Models;


namespace MediaHub.Tests;

public class MediaFileFactoryTests
{
    [Theory]
    [InlineData("song.mp3")]
    [InlineData("song.FLAC")]
    public void Create_AudioExtension_ReturnsAudioTrack(string path) =>
        Assert.IsType<AudioTrack>(MediaFileFactory.Create(path));

    [Fact]
    public void Create_Mp4_ReturnsVideoFile() =>
        Assert.IsType<VideoFile>(MediaFileFactory.Create("movie.mp4"));

    [Fact]
    public void Create_UnknownExtension_ReturnsNull() =>
        Assert.Null(MediaFileFactory.Create("notes.txt"));
}
