using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Experiments
{
    /// <summary>
    /// Encodes and decodes number strings where between 2 to 4 chars (dependent on their value) are
    /// mapped to a single byte.
    /// </summary>
    /// <remarks>
    /// The numbers 0-15 (0000-1111) can be stored stored using 4 bits and thus two of these numbers (as a string)
    /// can be stored in a single byte.
    /// 
    /// This algorithm uses 4-bit packing to store numbers in sudo-hex format into a byte[].
    /// 
    /// e.g. "37156" gets encoded into 2 bytes as follows:
    /// 1) Converts message to sudo-hex format => "37F6" (Standard hex format would yield "9124").
    /// 2) Bit packs "37" and "F6" into a single byte each.
    /// 2.1) "37" => 3 in binary using 4 bits is 0011, 7 is 0111,
    ///      and when bit-packed gives 00110111 and as a byte is 55.
    /// 2.2) "F6" => F in binary using 4 bits is 1111, 6 is 0110,
    ///      and when bit-packed gives 11110110 and as a byte is 246.
    /// 3) The final byte[] is { 55, 246 } which is less bytes, i.e. 2, than the original 5 byte string "37156".
    ///
    /// The Header:
    /// An extra byte is prepended to indicate whether the least significant 4-bits is a valid value.
    /// </remarks>
    public static class NumberStringEncoder
    {
        private const int HeaderLength = 1;

        private static readonly Regex OnlyDigitsRegex = new("^[0-9]+$");

        private static readonly Dictionary<string, string> HexCharMap = new()
        {
            {"A", "10"},
            {"B", "11"},
            {"C", "12"},
            {"D", "13"},
            {"E", "14"},
            {"F", "15"}
        };

        /// <summary>
        /// Encodes the number string.
        /// </summary>
        /// <param name="numbers">The number string to encode.</param>
        /// <returns>The encoded number string as bytes.</returns>
        public static byte[] Encode(string numbers)
        {
            var foo = OnlyDigitsRegex.IsMatch(numbers);
            if (!OnlyDigitsRegex.IsMatch(numbers))
                throw new ArgumentException("Only digits are allowed in the input string.");

            // Convert to sudo-hex to make iteration over the number string simpler.
            var sudoHex = ToSudoHex(numbers);

            // The algorithm ensures that the length of the encoded numbers is at most half the original size.
            var halfSudoHexLen = sudoHex.Length * 0.5;

            // If sudoHex length is odd then ensure we cater for the 4 least significant bits.
            var bytesLen = (int) Math.Ceiling(halfSudoHexLen);

            // True if sudoHex length is even i.e. all 4-bit sections are relevant the byte[].
            // False if sudoHex length is odd i.e. the 4 least significant bits are not relevant in the byte[].
            var header = (byte) (sudoHex.Length % 2 == 0 ? 1 : 0);

            var bytes = new byte[HeaderLength + bytesLen];
            bytes[0] = header;

            // Used to packs each 4-bit number in a byte. It is incremented only once a byte is complete.
            var packIndex = 1;

            for (var i = 0; i < sudoHex.Length; i++)
            {
                // Convert to string as we don't want to store the int value of the char, but rather the number value itself.  
                var value = $"{sudoHex[i]}";

                // Try convert from sudo-hex to a number i.e. if "A" return "10"
                if (HexCharMap.TryGetValue(value, out var fromSudoHex))
                {
                    value = fromSudoHex;
                }

                // Get the value as a byte. 
                var byt = Convert.ToByte(value);

                var applyBitPacking = i % 2 == 0;
                if (applyBitPacking)
                {
                    // The first value is left bit shifted, by 4 bits, into the most significant bits of the byte.
                    // e.g. if value = "9"
                    // => byt = 00001001
                    // => bytes[bitPackIdx] = 10010000.
                    bytes[packIndex] = (byte) (byt << 4);
                }
                else
                {
                    // The second value placed as the least significant bits byte using the bitwise | operator.
                    // e.g. if bytes[bitPackIdx] = 10010000 and the next value = "3"
                    // => byt = 00000011
                    // => bytes[bitPackIdx] = 10010011. 
                    bytes[packIndex] = (byte) (bytes[packIndex] | byt);

                    // Only increment the packIndex once we have processed two chars in the string. 
                    packIndex++;
                }
            }

            return bytes;
        }

        /// <summary>
        /// Decodes the number string.
        /// </summary>
        /// <param name="encoded">The number string to decode.</param>
        /// <returns>The decoded number string.</returns>
        public static string Decode(IReadOnlyList<byte> encoded)
        {
            var decoded = new StringBuilder();

            var isLeastSignificantBitsValid = encoded[0] == 1;
            var withoutHeaderLen = encoded.Count - HeaderLength;

            for (var i = HeaderLength; i < withoutHeaderLen + HeaderLength; i++)
            {
                var parts = encoded[i];

                // Get the most significant bits with a right bit shift.
                var part1 = parts >> 4;

                // Get the least significant bits with the bitwise & operator.
                // 0x0F = 00001111 acts as a mask when used with the bitwise & operator.
                // This yields the 4 least significant bits of the byte.
                // e.g. if parts = 10011011 then parts & 0x0F = 00001011 i.e. 10011011 & 00001111 = 00001011.
                var part2 = parts & 0x0F;

                var isLastByte = i == withoutHeaderLen;
                var part2IsSet = !isLastByte || isLeastSignificantBitsValid;

                decoded.Append(part2IsSet ? $"{part1}{part2}" : $"{part1}");
            }

            return decoded.ToString();
        }

        /// <summary>
        /// Converts "10", "11", "12", "13", "14", "15" into single hex characters "A", "B", "C", "D", "E", "F" respectively.
        /// </summary>
        /// <param name="numbers">The number string to convert.</param>
        /// <returns>The number string in sudo-hex format.</returns>
        /// <remarks>
        /// The name of the method indicates that the number string is not converted into the standard hex format,
        /// but rather a hex-like format.
        /// e.g.
        ///     "10215" in hex is "27E7"
        ///     "10215" in sudo-hex is "A2F"
        /// </remarks>
        public static string ToSudoHex(string numbers)
        {
            foreach (var (ch, val) in HexCharMap)
            {
                numbers = numbers.Replace(val, ch);
            }

            return numbers;
        }
    }
}