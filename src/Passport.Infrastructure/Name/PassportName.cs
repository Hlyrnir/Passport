namespace Passport.Infrastructure.Name
{
    internal enum PassportColumn
    {
        ConcurrencyStamp,
        CreatedAt,
        EditedAt,
        ExpiredAt,
        HasPermissionToCommand,
        HasPermissionToQuery,
        HolderId,
        Id,
        IsAuthority,
        IsEnabled,
        IssuedBy,
        LastCheckedAt,
        LastCheckedBy
    }

    internal enum PassportVisaRegisterColumn
    {
        Id,
        PassportId,
        PassportVisaId,
        RegisteredAt
    }

    internal enum PassportTable
    {
        Passport
    }

    internal enum PassportVisaRegisterTable
    {
        PassportVisaRegister
    }
}
