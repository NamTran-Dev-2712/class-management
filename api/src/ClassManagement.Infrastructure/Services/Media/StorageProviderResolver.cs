using ClassManagement.Application.Exceptions;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Media;

// Selects the storage adapter. When Storage:UseFakeProvider is on (dev/test) everything resolves to the
// local-disk backend so the flow runs offline; in production it maps to the cloud adapter. ActiveProvider
// reports which backend new uploads are recorded against. Mirrors PaymentProviderResolver (MVP-8).
public sealed class StorageProviderResolver : IStorageProviderResolver
{
    private readonly StorageOptions _options;
    private readonly LocalStorageProvider _local;
    private readonly R2StorageProvider _r2;

    public StorageProviderResolver(
        IOptions<StorageOptions> options,
        LocalStorageProvider local,
        R2StorageProvider r2
    )
    {
        _options = options.Value;
        _local = local;
        _r2 = r2;
    }

    public StorageProvider ActiveProvider =>
        _options.UseFakeProvider ? StorageProvider.Local : StorageProvider.R2;

    public IStorageProvider Resolve(StorageProvider provider)
    {
        if (_options.UseFakeProvider)
            return _local;

        return provider switch
        {
            StorageProvider.Local => _local,
            StorageProvider.R2 => _r2,
            _ => throw new BadException("Media.StorageProviderNotSupported"),
        };
    }
}
