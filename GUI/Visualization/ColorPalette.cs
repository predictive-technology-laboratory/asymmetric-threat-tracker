using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using LAIR.Extensions;

namespace PTL.ATT.GUI.Visualization
{
    public class ColorPalette
    {
        private static List<Color> _colors;

        static ColorPalette()
        {
            InitColors();
        }

        private static void InitColors()
        {
            string[] colorCodes = new string[] {
                "FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF",
                "800000", "C000C0", "00C0C0", "E000E0"
            };

            _colors = new List<Color>();
            foreach (string code in colorCodes)
                _colors.Add(ColorTranslator.FromHtml("#" + code));

            _colors.Randomize(new Random(1));
        }

        public static Color GetColor()
        {
            lock (_colors)
            {
                if (_colors.Count == 0)
                    InitColors();

                Color c = _colors[0];

                _colors.RemoveAt(0);

                return c;
            }
        }

        public static void ReturnColor(Color c)
        {
            if (c.Equals(Color.Black))
                return;

            lock (_colors)
            {
                _colors.Insert(0, c);
            }
        }
    }
}
