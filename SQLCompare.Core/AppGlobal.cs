namespace SQLCompare.Core
{
    /// <summary>
    /// Global configuration of the application
    /// </summary>
    public static class AppGlobal
    {
        /// <summary>
        /// Gets a value indicating whether the solution configuration is in Debug
        /// </summary>
        public static bool IsDevelopment
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets the initial port for the range of the WebServer
        /// </summary>
        public static int StartPortRange => 5000;

        /// <summary>
        /// Gets the final port for the range of the WebServer
        /// </summary>
        public static int EndPortRange => 6000;

        /// <summary>
        /// Gets the header attribute name for the authentication
        /// </summary>
        public static string AuthorizationHeaderName => "CustomAuthToken";
    }
}
