// ;
using VRage.Game.ModAPI;
using VRageMath;

namespace ExShared
{
    public static class Utils
    {
        public const string COLOR_TAG_DEFAULT_VAL = "<color=128,128,128,72>";
        public const string COLOR_TAG_READONLY_VAL = "<color=32,223,223,200>";
        public const string COLOR_TAG_NUMBER = "<color=223,223,32>";
        public const string COLOR_TAG_BOOL_TRUE = "<color=32,223,32>";
        public const string COLOR_TAG_BOOL_FALSE = "<color=223,32,32>";

        public static string FormatDistanceAsString(float _distance)
        {
            if (_distance > 1000.0f)
            {
                _distance /= 1000.0f;
                return string.Format("{0:F1}km", _distance);
            }

            return string.Format("{0:F1}m", _distance);
        }

        public static string FormatPercent(float _percent)
        {
            if (_percent == 0.0f)
                return "0%";
            if (_percent == 1.0f)
                return "100%";
            if (_percent == -1.0f)
                return "-100%";

            if (_percent > 0.0f && _percent < 0.001f)
                return "0.1%";
            if (_percent < 0.0f && _percent > -0.001f)
                return "-0.1%";

            if (_percent > 0.999)
                return "99.9%";
            if (_percent < -0.999)
                return "-99.9%";

            return string.Format("{0:F1}%", _percent * 100.0f);
        }

        public static Color CalculateBGColor(Color _color, float _opacity)
        {
            // SK: Stolen stuff
            // https://github.com/THDigi/BuildInfo/blob/master/Data/Scripts/BuildInfo/Utilities/Utils.cs#L256-L263
            //_color *= _opacity * _opacity * 1.075f;
            _color *= _opacity * _opacity * 0.90f;
            _color.A = (byte)(_opacity * 255.0f);

            return _color;
        }

        public static string GetCharacterName(IMyCharacter _character)
        {
            if (_character == null)
                return "null";
            if (_character.DisplayName != string.Empty)
                return _character.DisplayName;
            return _character.Name;
        }
    }

}
