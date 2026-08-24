namespace Common.API.Caching;

public sealed record CatalogCacheOptions(TimeSpan MaxAge, TimeSpan StaleWhileRevalidate);
