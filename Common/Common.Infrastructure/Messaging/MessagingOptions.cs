namespace Common.Infrastructure.Messaging;

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    public string Host { get; init; } = "localhost";

    public ushort Port { get; init; } = 5672;

    public string VirtualHost { get; init; } = "/";

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
