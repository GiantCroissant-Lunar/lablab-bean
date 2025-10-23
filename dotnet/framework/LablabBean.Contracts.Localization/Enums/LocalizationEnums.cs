namespace LablabBean.Contracts.Localization;

/// <summary>
/// Localization data formats supported by providers
/// </summary>
public enum LocalizationFormat
{
    /// <summary>
    /// JSON format (key-value pairs)
    /// </summary>
    Json,

    /// <summary>
    /// CSV format (comma-separated values)
    /// </summary>
    Csv,

    /// <summary>
    /// YAML format
    /// </summary>
    Yaml,

    /// <summary>
    /// XML format
    /// </summary>
    Xml,

    /// <summary>
    /// Properties/INI format
    /// </summary>
    Properties,

    /// <summary>
    /// XLIFF format (XML Localization Interchange File Format)
    /// </summary>
    Xliff,

    /// <summary>
    /// PO/POT format (Portable Object)
    /// </summary>
    Po,

    /// <summary>
    /// Custom binary format
    /// </summary>
    Binary,

    /// <summary>
    /// Custom format
    /// </summary>
    Custom
}

/// <summary>
/// Localization loading strategies
/// </summary>
public enum LocalizationLoadStrategy
{
    /// <summary>
    /// Load all translations at startup
    /// </summary>
    PreloadAll,

    /// <summary>
    /// Load translations when locale is selected
    /// </summary>
    LoadOnDemand,

    /// <summary>
    /// Load translations asynchronously in background
    /// </summary>
    BackgroundLoad,

    /// <summary>
    /// Stream translations as needed (for very large datasets)
    /// </summary>
    Streaming
}

/// <summary>
/// Text direction for locales
/// </summary>
public enum TextDirection
{
    /// <summary>
    /// Left-to-right text direction (English, etc.)
    /// </summary>
    LeftToRight,

    /// <summary>
    /// Right-to-left text direction (Arabic, Hebrew, etc.)
    /// </summary>
    RightToLeft,

    /// <summary>
    /// Top-to-bottom text direction (traditional Chinese, Japanese, etc.)
    /// </summary>
    TopToBottom
}

/// <summary>
/// Localization error severity levels
/// </summary>
public enum LocalizationErrorLevel
{
    Info,
    Warning,
    Error,
    Critical
}
