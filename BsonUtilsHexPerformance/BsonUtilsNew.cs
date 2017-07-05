using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace BsonUtilsHexPerformance
{
    /// <summary>
    /// A static class containing BSON utility methods.
    /// </summary>
    public static class BsonUtilsNew
    {
        // private static fields
        private static readonly uint[] __lookupByteToHex = CreateLookupByteToHex();
        private static readonly byte[] __lookupCharToByte = CreateLookupHexCharToByte();
        private static readonly char[] __lookupNibbleToHexChar = CreateLookupNibbleToHexChar();

        // private static methods
        /// <summary>
        /// Precalculates a lookup array that will help the performance of any byte to hex char conversion.
        /// </summary>
        /// <returns>An array that maps all potential byte values (0-255) to a uint that represents the two hex chars for each byte key.</returns>
        private static uint[] CreateLookupByteToHex()
        {
            var result = new uint[256];
            for (var b = 0; b < 256; b++)
            {
                // first we convert byte i to hex string...
                var s = b.ToString("x2");
                // ...then we store the hex representation of the byte in a uint such that
                // every uint in our array consists of 2x2 bytes
                // each of which will represent a single char, e.g.:
                //         b: 181
                //         s: "b5" --> two chars: 'b': 98 (01100010), '5': 53 (00110101)
                // result[i]:  3473506 (00000000 01100010 00000000 00110101) (little endian notation)
                //               this is 'b' ----^     and '5' ----^
                result[b] = ((uint)s[0] << 16) | s[1];
            }
            return result;
        }

        /// <summary>
        /// Precalculates a lookup array that will help the performance of any hex char to byte conversion.
        /// </summary>
        /// <returns>An array that maps all potential hex char values (0-F) to their respective byte representation or 255 for invalid hex chars.</returns>
        private static byte[] CreateLookupHexCharToByte()
        {
            var hexChars = "0123456789abcdefABCDEF";
            var result = new byte[103];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = 255;
            }
            foreach (var c in hexChars)
            {
                result[c] = Convert.ToByte(c.ToString(), 16);
            }
            return result;
        }

        /// <summary>
        /// Precalculates a lookup array that will help the performance of any nibble to char conversion.
        /// </summary>
        /// <returns>An array that maps all potential hex characters (0-F) to the byte values (0-255) to a uint that represents the two hex chars for each byte key.</returns>
        private static char[] CreateLookupNibbleToHexChar()
        {
            var lookup = new char[16];
            for (var i = 0; i < 16; i++)
            {
                lookup[i] = (char)(i + (i < 10 ? '0' : 'a' - 10));
            }

            return lookup;
        }

        // public static methods
        /// <summary>
        /// Gets a friendly class name suitable for use in error messages.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A friendly class name.</returns>
        public static string GetFriendlyTypeName(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return type.Name;
            }

            var sb = new StringBuilder();
            sb.AppendFormat("{0}<", Regex.Replace(type.Name, @"\`\d+$", ""));
            foreach (var typeParameter in type.GetTypeInfo().GetGenericArguments())
            {
                sb.AppendFormat("{0}, ", GetFriendlyTypeName(typeParameter));
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(">");
            return sb.ToString();
        }

        /// <summary>
        /// Parses a hex string into its equivalent byte array.
        /// </summary>
        /// <param name="s">The hex string to parse.</param>
        /// <returns>The byte equivalent of the hex string.</returns>
        public static byte[] ParseHexString(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            // some magic in order to avoid creating a new string with a "0" glued to the start in case of an odd string
            var offset = s.Length & 1; // the offset will hold either 1 or zero depending on if we need to shift character processing by one or not
            var bytes = new byte[(s.Length + offset) / 2];
            if (offset == 1)
            {
                // process first character separately, prefixed with a '0'
                if (!TryParseHexChars('0', s[0], out bytes[0]))
                {
                    throw new FormatException(string.Format("Invalid hex string {0}. Problem with substring {1} starting at position 0", s, "0" + s[0]));
                }
            }
            for (var i = offset; i < bytes.Length; i++)
            {
                var startIndex = 2 * i - offset;
                if (!TryParseHexChars(s[startIndex], s[startIndex + 1], out bytes[i]))
                {
                    throw new FormatException(string.Format("Invalid hex string {0}. Problem with substring {1} starting at position {2}", s, s.Substring(startIndex, 2), startIndex));
                }
            }

            return bytes;
        }

        private static bool TryParseHexChars(char left, char right, out byte b)
        {
            var l = __lookupCharToByte[left];
            var r = __lookupCharToByte[right];
            if (l == 255 || r == 255)
            {
                b = 0;
                return false;
            }
            b = (byte)((l << 4) | r);
            return true;
        }

        /// <summary>
        /// Converts a value to a hex character.
        /// </summary>
        /// <param name="value">The value (assumed to be between 0 and 15).</param>
        /// <returns>The hex character.</returns>
        public static char ToHexChar(int value)
        {
            return __lookupNibbleToHexChar[value];
        }

        /// <summary>
        /// Converts a byte array to a hex string.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>A hex string.</returns>
        public static string ToHexString(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            var c = new char[bytes.Length * 2];

            for (var i = 0; i < bytes.Length; i++)
            {
                var val = __lookupByteToHex[bytes[i]];
                c[2 * i] = (char)(val >> 16);
                c[2 * i + 1] = (char)val;
            }

            return new string(c);
        }

        /// <summary>
        /// Converts a DateTime to local time (with special handling for MinValue and MaxValue).
        /// </summary>
        /// <param name="dateTime">A DateTime.</param>
        /// <returns>The DateTime in local time.</returns>
        public static DateTime ToLocalTime(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Local);
            }
            else if (dateTime == DateTime.MaxValue)
            {
                return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Local);
            }
            else
            {
                return dateTime.ToLocalTime();
            }
        }

        /// <summary>
        /// Tries to parse a hex string to a byte array.
        /// </summary>
        /// <param name="s">The hex string.</param>
        /// <param name="bytes">A byte array.</param>
        /// <returns>True if the hex string was successfully parsed.</returns>
        public static bool TryParseHexString(string s, out byte[] bytes)
        {
            try
            {
                bytes = ParseHexString(s);
            }
            catch
            {
                bytes = null;
                return false;
            }

            return true;
        }
    }
}