using System.Text;
using Common.Application.Images;
using FluentAssertions;
using Xunit;

namespace Common.Application.UnitTests.Images;

public sealed class ImageValidatorTests
{
    private const long MaxSize = 1024;

    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x01];
    private static readonly byte[] Webp =
        [0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50];

    private static readonly byte[] Gif = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00];

    private readonly ImageValidator _validator = new();

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpeg; charset=binary")]
    [InlineData("IMAGE/JPEG")]
    public void Validate_WithAJpegDeclaredAsJpeg_Accepts(string declaredContentType) =>
        _validator.Validate(new ImageContent(Jpeg, declaredContentType, MaxSize))
            .Should().Be(ImageRejection.None);

    [Fact]
    public void Validate_WithAPngDeclaredAsPng_Accepts() =>
        _validator.Validate(new ImageContent(Png, "image/png", MaxSize))
            .Should().Be(ImageRejection.None);

    [Fact]
    public void Validate_WithAWebpDeclaredAsWebp_Accepts() =>
        _validator.Validate(new ImageContent(Webp, "image/webp", MaxSize))
            .Should().Be(ImageRejection.None);

    [Fact]
    public void Validate_WithAnSvgDeclaredAsPng_RejectsTheFormat() =>
        _validator.Validate(new ImageContent(Svg(), "image/png", MaxSize))
            .Should().Be(ImageRejection.UnsupportedFormat);

    [Fact]
    public void Validate_WithAnSvgDeclaredAsSvg_RejectsTheFormat() =>
        _validator.Validate(new ImageContent(Svg(), "image/svg+xml", MaxSize))
            .Should().Be(ImageRejection.UnsupportedFormat);

    [Fact]
    public void Validate_WithAPngDeclaredAsSvg_RejectsTheMismatch() =>
        _validator.Validate(new ImageContent(Png, "image/svg+xml", MaxSize))
            .Should().Be(ImageRejection.UnsupportedFormat);

    [Fact]
    public void Validate_WithAJpegDeclaredAsPng_RejectsTheMismatch() =>
        _validator.Validate(new ImageContent(Jpeg, "image/png", MaxSize))
            .Should().Be(ImageRejection.UnsupportedFormat);

    [Fact]
    public void Validate_WithAGif_RejectsTheFormat() =>
        _validator.Validate(new ImageContent(Gif, "image/gif", MaxSize))
            .Should().Be(ImageRejection.UnsupportedFormat);

    [Fact]
    public void Validate_WithoutADeclaredType_FallsBackToTheBytes() =>
        _validator.Validate(new ImageContent(Png, null, MaxSize))
            .Should().Be(ImageRejection.None);

    [Fact]
    public void Validate_WithAGenericDeclaredType_FallsBackToTheBytes() =>
        _validator.Validate(new ImageContent(Png, "application/octet-stream", MaxSize))
            .Should().Be(ImageRejection.None);

    [Fact]
    public void Validate_WithAGenericDeclaredTypeOverAnSvg_StillRejectsTheFormat() =>
        _validator.Validate(new ImageContent(Svg(), "application/octet-stream", MaxSize))
            .Should().Be(ImageRejection.UnsupportedFormat);

    [Fact]
    public void Validate_WithAnEmptyBody_RejectsAsEmpty() =>
        _validator.Validate(new ImageContent(ReadOnlyMemory<byte>.Empty, "image/png", MaxSize))
            .Should().Be(ImageRejection.Empty);

    [Fact]
    public void Validate_BeyondTheSizeCap_RejectsAsTooLarge() =>
        _validator.Validate(new ImageContent(Oversized(), "image/png", MaxSize))
            .Should().Be(ImageRejection.TooLarge);

    [Fact]
    public void Validate_ExactlyAtTheSizeCap_Accepts() =>
        _validator.Validate(new ImageContent(Sized((int)MaxSize), "image/png", MaxSize))
            .Should().Be(ImageRejection.None);

    [Fact]
    public void Detect_WithATruncatedRiffHeader_ReturnsNull() =>
        _validator.Detect([0x52, 0x49, 0x46, 0x46, 0x00]).Should().BeNull();

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    public void Validate_WithAnOversizedFileOfAnyType_PrefersTheSizeVerdict(string declaredContentType) =>
        _validator.Validate(new ImageContent(Oversized(), declaredContentType, MaxSize))
            .Should().Be(ImageRejection.TooLarge);

    private static byte[] Svg() =>
        Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>");

    private static byte[] Oversized() => Sized((int)MaxSize + 1);

    private static byte[] Sized(int length)
    {
        byte[] content = new byte[length];
        Png.CopyTo(content, 0);

        return content;
    }
}
