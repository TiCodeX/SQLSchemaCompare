namespace SQLCompare.Core
{
    public static class AppGlobal
    {
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

        // Maybe to be refactored
        public static int StartPortRange => 5000;

        public static int EndPortRange => 6000;

        public static string AuthorizationHeaderName => "CustomAuthToken";
    }
}
