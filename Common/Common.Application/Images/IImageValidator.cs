namespace Common.Application.Images;

public sealed record ImageContent(
    ReadOnlyMemory<byte> Bytes,
    string? DeclaredContentType,
    long MaxSizeBytes);

public interface IImageValidator
{
    ImageRejection Validate(ImageContent content);

    ImageFormat? Detect(ReadOnlySpan<byte> bytes);
}
