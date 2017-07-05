using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace BsonUtilsHexPerformance
{
    /// <summary>
    /// A static class containing BSON utility methods.
    /// </summary>
    public static class BsonUtilsOld
    {
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

            byte[] bytes;
            if ((s.Length & 1) != 0)
            {
                s = "0" + s; // make length of s even
            }
            bytes = new byte[s.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string hex = s.Substring(2 * i, 2);
                try
                {
                    byte b = Convert.ToByte(hex, 16);
                    bytes[i] = b;
                }
                catch (FormatException e)
                {
                    throw new FormatException(
                        string.Format("Invalid hex string {0}. Problem with substring {1} starting at position {2}",
                            s,
                            hex,
                            2 * i),
                        e);
                }
            }

            return bytes;
        }
        
        /// <summary>
        /// Converts a value to a hex character.
        /// </summary>
        /// <param name="value">The value (assumed to be between 0 and 15).</param>
        /// <returns>The hex character.</returns>
        public static char ToHexChar(int value)
        {
            return (char)(value + (value < 10 ? '0' : 'a' - 10));
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

            var length = bytes.Length;
            var c = new char[length * 2];

            for (int i = 0, j = 0; i < length; i++)
            {
                var b = bytes[i];
                c[j++] = ToHexChar(b >> 4);
                c[j++] = ToHexChar(b & 0x0f);
            }

            return new string(c);
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
