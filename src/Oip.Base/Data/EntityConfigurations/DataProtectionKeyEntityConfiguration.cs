using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;

namespace Oip.Data.EntityConfigurations;

/// <summary>
/// Configures the entity for data protection keys in the database
/// </summary>
/// <param name="database">The database facade for checking provider</param>
/// <param name="schema">The schema name for the table</param>
public class DataProtectionKeyEntityConfiguration(DatabaseFacade database, string schema)
    : IEntityTypeConfiguration<DataProtectionKey>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
    {
        builder.SetTableWithSchema(database, schema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.HasData(new DataProtectionKey
        {
            Id = 1,
            FriendlyName = "key-b57db670-000b-4dd7-8603-e82a29941b6d",
            Xml =
                "<key id=\"b57db670-000b-4dd7-8603-e82a29941b6d\" version=\"1\"><creationDate>2025-12-31T12:29:13.551409Z</creationDate><activationDate>2025-12-31T12:29:13.395011Z</activationDate><expirationDate>2125-12-07T12:29:13.395011Z</expirationDate><descriptor deserializerType=\"Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60\"><descriptor><encryption algorithm=\"AES_256_CBC\" /><validation algorithm=\"HMACSHA256\" /><masterKey p4:requiresEncryption=\"true\" xmlns:p4=\"http://schemas.asp.net/2015/03/dataProtection\"><!-- Warning: the key below is in an unencrypted form. --><value>y6SO3S3B9NggxGQguIPpg3kCnMYSJt9bed1u8VFDIOqmL4eyzvBZLuLHJY74ASdSAlNW5ayiRDWnWhEzc95IcA==</value></masterKey></descriptor></descriptor></key>"
        });
    }
}