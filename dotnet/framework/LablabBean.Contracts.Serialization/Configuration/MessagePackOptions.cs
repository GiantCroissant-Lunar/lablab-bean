using MessagePack;
using MessagePack.Resolvers;

namespace LablabBean.Contracts.Serialization.Configuration;

public class MessagePackOptions
{
    public MessagePackCompression Compression { get; set; } = MessagePackCompression.Lz4BlockArray;
    public IFormatterResolver? Resolver { get; set; } = ContractlessStandardResolver.Instance;
    public bool AllowAssemblyVersionMismatch { get; set; } = false;
    public MessagePackSecurity Security { get; set; } = MessagePackSecurity.UntrustedData;
    public bool OmitAssemblyVersion { get; set; } = false;

    public MessagePackSerializerOptions ToSerializerOptions()
    {
        var options = MessagePackSerializerOptions.Standard;

        if (Resolver != null)
            options = options.WithResolver(Resolver);

        options = options.WithCompression(Compression);
        options = options.WithSecurity(Security);
        options = options.WithAllowAssemblyVersionMismatch(AllowAssemblyVersionMismatch);
        options = options.WithOmitAssemblyVersion(OmitAssemblyVersion);

        return options;
    }

    public static MessagePackOptions CreateDefault()
    {
        return new MessagePackOptions
        {
            Compression = MessagePackCompression.Lz4BlockArray,
            Resolver = ContractlessStandardResolver.Instance,
            Security = MessagePackSecurity.UntrustedData,
            AllowAssemblyVersionMismatch = false,
            OmitAssemblyVersion = false
        };
    }

    public static MessagePackOptions CreateHighPerformance()
    {
        return new MessagePackOptions
        {
            Compression = MessagePackCompression.None,
            Resolver = StandardResolver.Instance,
            Security = MessagePackSecurity.TrustedData,
            AllowAssemblyVersionMismatch = true,
            OmitAssemblyVersion = true
        };
    }

    public static MessagePackOptions CreateCompact()
    {
        return new MessagePackOptions
        {
            Compression = MessagePackCompression.Lz4BlockArray,
            Resolver = ContractlessStandardResolver.Instance,
            Security = MessagePackSecurity.UntrustedData,
            AllowAssemblyVersionMismatch = false,
            OmitAssemblyVersion = true
        };
    }
}
