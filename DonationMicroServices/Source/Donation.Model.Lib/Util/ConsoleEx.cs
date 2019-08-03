using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Donation.Model.Lib.Util
{
    public class ConsoleEx
    {
        public static IEnumerable<ConsoleColor> GetConsoleColorList()
        {
            var l = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToList();
            l.Remove(ConsoleColor.Black); // Remove too dark color
            l.Remove(ConsoleColor.DarkBlue);
            return l;
        }

        private static List<ConsoleColor> _availableColors;
        private static List<ConsoleColor> AvailableColors
        {
            get
            {
                if (_availableColors == null)
                    _availableColors = GetConsoleColorList().ToList();
                return _availableColors;
            }
        }

        private static Dictionary<string, ConsoleColor> ColorKeyMap = new Dictionary<string, ConsoleColor>();

        public static void WriteLineAutoColor(string text, string key)
        {
            ConsoleColor c;
            key = key.ToLowerInvariant();
            if (ColorKeyMap.ContainsKey(key))
            {
                c = ColorKeyMap[key];
            }
            else
            {
                if (AvailableColors.Count > 0)
                {
                    c = AvailableColors[0];
                    ColorKeyMap.Add(key, c);
                    AvailableColors.RemoveAt(0);
                }
                else c = ConsoleColor.DarkGray;
            }
            WriteLine(text, c);
        }

        public static void Pause(string message = "Hit any key to continue")
        {
            Console.WriteLine(message);
            var k = Console.ReadKey(true);
        }

        public static void WriteLine(string text, ConsoleColor color)
        {
            var bu = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(text);

            Console.ForegroundColor = bu;
        }
    }
}
