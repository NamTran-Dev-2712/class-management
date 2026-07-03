namespace ClassManagement.Domain.Modules.Media.Enums;

// Supported object-storage backends (MVP-9). Stored as PascalCase text. The concrete adapter is chosen
// by IStorageProviderResolver (Local for dev/test, R2 for prod). See MediaAssetConfiguration check.
public enum StorageProvider
{
    Local,
    R2,
}
