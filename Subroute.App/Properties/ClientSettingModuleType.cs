namespace Subroute.App.Properties
{
    /// <summary>
    /// Indicates what type of client side module to output.
    /// </summary>
    public enum ClientSettingModuleType
    {
        /// <summary>
        /// Create a module that is compatible with loaders such as RequireJS.
        /// </summary>
        Amd,

        /// <summary>
        /// Create a module that contains a basic json structure containing settings.
        /// </summary>
        Json
    }
}