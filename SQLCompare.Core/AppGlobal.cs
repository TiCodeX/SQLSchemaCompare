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
    }
}
