using SQLite;

namespace assignment_2425
{
    public static class Constants
    {
        private const string DatabaseFilename = "SmartFreeze.db3";

        public const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }
}
