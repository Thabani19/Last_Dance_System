using System;

namespace Last_Dance_System.Models
{
    public static class LessonCodeGenerator
    {
        public static string GenerateCode()
        {
            // Generates a 6-digit code
            return new Random().Next(100000, 999999).ToString();
        }
    }
}