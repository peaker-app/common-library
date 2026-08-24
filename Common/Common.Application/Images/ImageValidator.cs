using System.Collections.Frozen;

namespace Common.Application.Images;

public sealed class ImageValidator : IImageValidator
{
    private const string ImagePrefix = "image/";

    private static readonly byte[] JpegMagic = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngMagic = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] RiffMagic = [0x52, 0x49, 0x46, 0x46];
    private static readonly byte[] WebpMagic = [0x57, 0x45, 0x42, 0x50];

    private static readonly FrozenDictionary<ImageFormat, string> ContentTypes =
        new Dictionary<ImageFormat, string>
        {
            [ImageFormat.Jpeg] = "image/jpeg",
            [ImageFormat.Png] = "image/png",
            [ImageFormat.WebP] = "image/webp"
        }.ToFrozenDictionary();

    public ImageRejection Validate(ImageContent content)
    {
        if (content.Bytes.IsEmpty)
        {
            return ImageRejection.Empty;
        }

        if (content.Bytes.Length > content.MaxSizeBytes)
        {
            return ImageRejection.TooLarge;
        }

        ImageFormat? format = Detect(content.Bytes.Span);

        return format is not null && Matches(format.Value, content.DeclaredContentType)
            ? ImageRejection.None
            : ImageRejection.UnsupportedFormat;
    }

    public ImageFormat? Detect(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith(JpegMagic))
        {
            return ImageFormat.Jpeg;
        }

        if (bytes.StartsWith(PngMagic))
        {
            return ImageFormat.Png;
        }

        return IsWebp(bytes) ? ImageFormat.WebP : null;
    }

    private static bool IsWebp(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 12 && bytes.StartsWith(RiffMagic) && bytes.Slice(8, 4).SequenceEqual(WebpMagic);

    private static bool Matches(ImageFormat format, string? declaredContentType)
    {
        if (declaredContentType is null)
        {
            return true;
        }

        string declared = Normalize(declaredContentType);

        return !declared.StartsWith(ImagePrefix, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ContentTypes[format], declared, StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string declaredContentType)
    {
        int separator = declaredContentType.IndexOf(';', StringComparison.Ordinal);

        return (separator < 0 ? declaredContentType : declaredContentType[..separator]).Trim();
    }
}
