using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;

/*
namespace RefactoringLegacyCode.Tests
{
    /* [TestFixture]
     public class ProductEncoderTests
     {
         [Test]
         public void ProductEncodeDecode_ShouldBeReversible()
         {
             Prop.ForAll<string, int>((name, quantity) =>
             {
                 if (string.IsNullOrEmpty(name) || quantity < 0) return true;

                 var product = new Product { Name = name, Quantity = quantity };
                 var encoded = product.Encode();
                 var decoded = Product.Decode(encoded);

                 return decoded.Equals(product);
             }).QuickCheckThrowOnFailure();
         }
     }

    public static class CustomEncoder
    {
        private const string Prefix = "ENC"; // Prefix for encoded strings
        private const string Suffix = "XYZ"; // Suffix for encoded strings
        private const int ShiftOffset = 2;  // Shift characters by 2

        // Custom encoding logic
        public static string Encode(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Step 1: Reverse the string
            var reversed = new string(input.Reverse().ToArray());

            // Step 2: Shift characters
            var shifted = new string(reversed.Select(c =>
            {
                if (char.IsLetter(c)) return (char)(c + ShiftOffset); // Shift letters
                if (char.IsDigit(c)) return (char)(c + ShiftOffset); // Shift digits
                return c; // Leave other characters unchanged
            }).ToArray());

            // Step 3: Add prefix and suffix
            return $"{Prefix}{shifted}{Suffix}";
        }

        // Custom decoding logic
        public static string Decode(string encodedInput)
        {
            if (string.IsNullOrEmpty(encodedInput)) return encodedInput;

            // Step 1: Remove prefix and suffix
            if (!encodedInput.StartsWith(Prefix) || !encodedInput.EndsWith(Suffix))
                throw new ArgumentException("Invalid encoded input format");

            var stripped = encodedInput.Substring(Prefix.Length, encodedInput.Length - Prefix.Length - Suffix.Length);

            // Step 2: Reverse the shift
            var unshifted = new string(stripped.Select(c =>
            {
                if (char.IsLetter(c)) return (char)(c - ShiftOffset); // Reverse shift for letters
                if (char.IsDigit(c)) return (char)(c - ShiftOffset); // Reverse shift for digits
                return c; // Leave other characters unchanged
            }).ToArray());

            // Step 3: Reverse the string
            return new string(unshifted.Reverse().ToArray());
        }
    }

    [TestFixture]
    public class CustomEncoderTests
    {
        [Test]
        public void EncodeDecode_ShouldBeReversible()
        {
            Prop.ForAll<string>(input =>
            {
                var encoded = CustomEncoder.Encode(input);
                var decoded = CustomEncoder.Decode(encoded);
                return decoded == input;
            }).QuickCheckThrowOnFailure();
        }
    }
}
*/