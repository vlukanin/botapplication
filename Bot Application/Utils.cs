namespace Bot_Application
{
    public static class Utils
    {
        public static string NextTo(this string[] str, string pat)
        {
            for (int i = 0; i < str.Length - 1; i++)
            {
                if (str[i].ToLower() == pat.ToLower()) return str[i + 1];
            }
            return string.Empty;
        }

        public static bool IsPresent(this string[] str, string pat)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i].ToLower() == pat.ToLower()) return true;
            }
            return false;
        }
    }
}