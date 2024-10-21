namespace Passport.Application.Default
{
    internal static class DefaultPassportVisa
    {
        public static class Name
        {
            public const string Passport = "PASSPORT";
        }

        public static class Level
        {
            public const int Read = 0;
            public const int Update = 1;
            public const int Create = 2;
            public const int Delete = 3;
        }
    }
}
