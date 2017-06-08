namespace Subroute.Container
{
    /// <summary>
    /// Directories setup for the current execution config.
    /// </summary>
    public class ExecutionDirectories
    {
        /// <summary>
        /// Root directory created to hold the app.config file for this execution.
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// Full file path to the app.config file.
        /// </summary>
        public string ConfigFile { get; set; }
    }
}