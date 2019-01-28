using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Telemetry.RealTime.Dashboard.Assets
{
    public static class ColorPalette
    {
        private static Dictionary<int, string> _Palette;

        static ColorPalette()
        {
            //TODO - move to config
            _Palette = new Dictionary<int, string>();
            _Palette.Add(0, "#859FA7");

            _Palette.Add(10, "#0073DA");
            _Palette.Add(11, "#0A89FB");

            _Palette.Add(20, "#903F72");
            _Palette.Add(21, "#B36D97");

            _Palette.Add(30, "#96C110");
            _Palette.Add(31, "#B3DB37");

            _Palette.Add(40, "#859FA7");
            _Palette.Add(41, "#BCCBD0");

            _Palette.Add(50, "#ff9618");
            _Palette.Add(51, "#FFAB46");
        }

        public static string GetHexFor(int column, int row)
        {
            var index = column * 10 + row;
            if (_Palette.ContainsKey(index))
                return _Palette[index];

            index = column * 10;
            if (_Palette.ContainsKey(index))
                return _Palette[index];

            return _Palette[0];
        }
    }
}