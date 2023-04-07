using System;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

//  DanStringExtensions
//
//  ver 1.17 07/04/2023 Added DefaultComparisonType to class
//	ver 1.16 05/01/2023 Added CamelCaseToSpaced method
//  ver 1.15 29/12/2022 Updated Before/After to have StringComparison parameter
//  ver 1.14 28/11/2022 Added FilterToLetters method
//  ver 1.13 30/10/2022 Fixed spelling of occurrence
//  ver 1.12 11/07/2022 ReSharper recommended optimizations
//  ver 1.11 07/06/2022 Added RemoveDoubleSpaces method
//  ver 1.10 03/06/2022 Fixed bug in ToInt32 method when faced with numbers larger than 2,147,483,647
//  ver 1.09 24/05/2022 Fixed bug in ContainsKeyword (couldn't handle keywords with spaces in them)
//  ver 1.08 05/05/2022 Added HidePassword method
//  ver 1.07 28/03/2022 Added Words (returns a list of words in the string)
//                       and added XML documentation to each method
//  ver 1.06 11/03/2022 Added GeneratePassword
//  ver 1.05 13/02/2022 Added .ToInt32()
//  ver 1.04 20/01/2022 Added IsNullOrEmpty as an extension
//  ver 1.03 05/01/2022 Fixed bug in keyword search (it was case sensitive, now it isn't)
//  ver 1.02 02/01/2022 Added Contains extension - check if a string occurs in a list
//  ver 1.01 14/08/2021 Removed Visual Basic Strings calls, now native c#
//  ver 1.00 06/04/2021 Initial version

namespace Dan;

internal static class StringExtensions
{

    public static StringComparison DefaultComparisonType = StringComparison.Ordinal; 

    public static class Sets
    {
        public const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string Numeric = "0123456789";
        public const string Symbol = "!@#$%^&*()-=_+[]{};':\"\\|,./<>?~`";
        public const string AlphaNumeric = Alphabet + Numeric;
    }

    /// <summary>
    /// Checks if this (Enumerable) contains a string (and supports the StringComparison option)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool Contains(this IEnumerable<string> source, string value, StringComparison comparisonType)
    {
        //Extension for case insensitive search of a hashset
        foreach (string item in source)
        {
            if (item.Equals(value, comparisonType)) { return true; }
        }
        return false;
    }

    /// <summary>
    /// Checks if this (Enumerable) contains a string (and supports the StringComparison option)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool Contains(this IEnumerable<string> source, string value)
    {
        return Contains(source, value, DefaultComparisonType);
    }

    /// <summary>
    /// Get the string between startString and endString
    /// (Extension method provided by DanStringExtensions)
    /// </summary>

    public static string Between(this string text, string startString, string endString, bool stripWhiteSpace = false, bool searchBackwards = false)
    {
        if (startString == null) { throw new ArgumentException("Parameter cannot be null", "startString"); }
        if (endString == null) { throw new ArgumentException("Parameter cannot be null", "endString"); }
        string textOriginal = text;
        text = text.ToLower();
        startString = startString.ToLower();
        endString = endString.ToLower();
        int loc;
        int loc2;

        if (searchBackwards)
        {
            loc = text.LastIndexOf(startString, DefaultComparisonType);
        }
        else
        {
            loc = text.IndexOf(startString, DefaultComparisonType);
        }
        if (loc == -1) { Log.Trace($"String.Between {startString} was not found in {text}"); return ""; }

        loc += startString.Length;

        if (searchBackwards)
        {
            loc2 = text.LastIndexOf(endString, loc, DefaultComparisonType);
        }
        else
        {
            loc2 = text.IndexOf(endString, loc, DefaultComparisonType);
        }
        if (loc2 == -1) { Log.Trace($"String.Between {endString} was not found in {text}"); return ""; }

        if (loc < 0 || loc >= textOriginal.Length)
        {
            return string.Empty;
        }

        string returnVal = textOriginal.Substring(loc, loc2 - loc);

        if (stripWhiteSpace)
        {
            returnVal = returnVal.Trim();
        }

        return returnVal;

    }

    /// <summary>
    /// Removes double whitespaces from a string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string RemoveDoubleSpaces(this string text)
    {
        return Regex.Replace(text, @"\s+", " ");
    }

    /// <summary>
    /// Get the left (x) characters of a string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string Left(this string text, int x)
    {
        if (x > text.Length) { x = text.Length; }
        return text.Substring(0, x);
    }

    /// <summary>
    /// Check if the left part of a string matches another string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool Left(this string text, string match)
    {
        int length = match.Length;
        if (length > text.Length) { return false; } // match is too long
        return (text.Substring(0, length) == match);
    }

    /// <summary>
    /// Get the right (x) characters of a string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string Right(this string text, int length)
    {
        if (length > text.Length) { length = text.Length; }
        return text.Substring(text.Length - length, length);
    }

    /// <summary>
    /// Check if the right part of a string matches another string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool Right(this string text, string match)
    {
        int length = match.Length;
        if (length > text.Length) { return false; } //match is too long
        return (text.Substring(text.Length - length, length) == match);
    }

    /// <summary>
    /// Get the part of the string before the occurrence of a specified search substring
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string Before(this string text, string searchSubstring,StringComparison? sc = null, bool stripWhiteSpace = false, bool searchBackwards = false)
    {
        sc ??= DefaultComparisonType;

        int loc;
        // for string after
        if (searchBackwards) //last occurrence
        {
            loc = text.LastIndexOf(searchSubstring,(StringComparison)sc);
        }
        else
        { //first occurrence
            loc = text.IndexOf(searchSubstring, (StringComparison)sc);
        }
        if (loc == -1) { return ""; }
        string returnVal = text.Substring(0, loc);
        if (stripWhiteSpace)
        {
            returnVal = returnVal.Trim();
        }

        return returnVal;
    }

    /// <summary>
    /// Get the remainder of the string after the occurrence of a specified search substring
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string After(this string text, string searchSubstring,StringComparison? sc = null, bool stripWhiteSpace = false, bool searchBackwards = false)
    {
        sc ??= DefaultComparisonType;
        int loc;
        // for string after
        if (searchBackwards) //last occurrence
        {
            loc = text.LastIndexOf(searchSubstring, (StringComparison) sc);
        }
        else
        { //first occurrence
            loc = text.IndexOf(searchSubstring, (StringComparison)sc);
        }
        if (loc == -1) { return ""; }

        string returnVal = text.Substring(loc + searchSubstring.Length);

        if (stripWhiteSpace)
        {
            returnVal = returnVal.Trim();
        }

        return returnVal;
    }

    /// <summary>
    /// Remove the right (x) characters from a string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string TrimRight(this string text, int lengthToRemove)
    {
        if (lengthToRemove > text.Length) { return ""; }
        return text.Substring(0, text.Length - lengthToRemove);
    }

    /// <summary>
    /// Remove the left (x) characters from a string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string TrimLeft(this string text, int lengthToRemove)
    {
        if (lengthToRemove > text.Length) { return ""; }
        return text.Substring(lengthToRemove);
    }

    /// <summary>
    /// Change all strings in an Enumerable to lower case
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static List<string> ToLower(this IEnumerable<string> strings)
    {
        List<string> result = new();
        foreach (string? s in strings)
        {
            result.Add(s.ToLower());
        }

        return result;
    }

    /// <summary>
    /// Change all strings in an Enumerable to upper case
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static List<string> ToUpper(this IEnumerable<string> strings)
    {
        List<string> result = new();
        foreach (string? s in strings)
        {
            result.Add(s.ToUpper());
        }

        return result;
    }

    /// <summary>
    /// Filter a string so that it only includes (or optionally excludes) the specified character set
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string FilterCharacters(this string text, string filter, bool exclude = false)
    {
        if (text.IsNullOrEmpty()) { return string.Empty; }
        StringBuilder returnVal = new();
        foreach (char c in text)
        {
            if (exclude)
            {
                if (!filter.Contains(c))
                {
                    returnVal.Append(c);
                }
            }
            else
            {
                if (filter.Contains(c))
                {
                    returnVal.Append(c);
                }
            }
        }
        return returnVal.ToString();
    }

    public static bool IsNullOrEmpty(this string text)
    {
        return (string.IsNullOrEmpty(text));
    }

    /// <summary>
    /// Check if a string only contains characters of the specified character set
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool IsCharacterSet(this string text, string characterSet)
    {
        if (text == "") { return false; } //blank input text
        foreach (char c in text)
        {
            bool match = false;
            foreach (char cs in characterSet)
            {
                if (c == cs)
                {
                    match = true;
                    break;
                }
            }
            if (!match) { return false; } //no match
        }
        return true;
    }

    /// <summary>
    /// Filter a string so it only contains letters
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string FilterToLetters(this string text)
    {
        if (text.IsNullOrEmpty()) { return string.Empty; }
        StringBuilder returnVal = new();
        foreach (char c in text)
        {
            if (char.IsLetter(c))
            {
                returnVal.Append(c);
            }
        }
        return returnVal.ToString();
    }


    /// <summary>
    /// Filter a string so it only contains digits
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string FilterToDigits(this string text)
    {
        string filter = "0123456789";
        return FilterCharacters(text, filter);
    }

    /// <summary>
    /// Try and convert a string to a decimal (returns 0 if it fails)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static decimal ToDecimal(this string text)
    {
        string filter = "0123456789.";
        string result = FilterCharacters(text, filter);
        if (result == "") { return 0M; }
        try
        {
            return Convert.ToDecimal(result);
        }
        catch { return 0M; }
    }

    /// <summary>
    /// Try and convert a string to a short (int16) (returns 0 if it fails)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static short ToInt16(this string text)
    {
        string filter = "0123456789.";
        string result = FilterCharacters(text, filter);
        if (result == "") { return 0; }
        if (result == ".") { return 0; }

        try
        {
            decimal value = Math.Round(Convert.ToDecimal(result));
            if (value is > 32767 or < -32768)
            {
                Log.Error($"Trying to convert {value} to Int16. it is too big (or too small)! Returning 0!");
                return 0;
            }

            return Convert.ToInt16(value);
        }
        catch
        {
            Log.Error($"Failed while trying to convert {result} to Int16. Returning 0!");
            return 0;
        }
    }

    /// <summary>
    /// Try and convert a string to a int (int32) (returns 0 if it fails)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static int ToInt32(this string text)
    {
        string filter = "0123456789.";
        string result = FilterCharacters(text, filter);
        if (result == "") { return 0; }
        if (result == ".") { return 0; }

        try
        {
            decimal value = Math.Round(Convert.ToDecimal(result));
            if (value is > 2_147_483_647 or < -2_147_483_648)
            {
                Log.Error($"Trying to convert {value} to Int32. it is too big (or too small)! Returning 0!");
                return 0;
            }

            return Convert.ToInt32(value);
        }
        catch
        {
            Log.Error($"Failed while trying to convert {result} to Int32. Returning 0!");
            return 0;
        }
    }

    /// <summary>
    /// Try and convert a string to a long (int64) (returns 0 if it fails)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static long ToInt64(this string text)
    {
        string filter = "0123456789.";
        string result = FilterCharacters(text, filter);
        if (result == "") { return 0; }
        if (result == ".") { return 0; }
        try
        {
            return (long)Math.Round(Convert.ToDecimal(result));
        }
        catch { return 0; }
    }

    /// <summary>
    /// Get a list of all occurrences of a search string in another string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static List<int> AllIndexesOf(this string str, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException(@"the string to find may not be empty", nameof(value));
        }

        List<int> indexes = new List<int>();
        for (int index = 0; ; index += value.Length)
        {
            index = str.IndexOf(value, index, DefaultComparisonType);
            if (index == -1)
            {
                return indexes;
            }

            indexes.Add(index);
        }
    }

    /// <summary>
    /// Get a list of all the words in this string (sentence)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static List<string> Words(this string str)
    {
        //find all the punctuation in the string
        char[] punctuation = str.Where(c => char.IsPunctuation(c) || char.IsWhiteSpace(c))
            .Distinct().ToArray();
        //split the string by punctuation (and then trim the punctuation)
        List<string> words = str.Split(punctuation).ToList();
        words.RemoveAll(s => s.IsNullOrEmpty());
        return words;
    }

    /// <summary>
    /// Check if a string contains a keyword (eg. "my nana likes bananas" - if we searched for nana, it would match the first nana, but not bananas)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool ContainsKeyword(this string text, string keyword, StringComparison? sc = null)
    {
        sc ??= DefaultComparisonType;
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(keyword))
        {
            return false;

        }

        if (keyword.Contains(' '))
        {
            return _ContainsKeyword(text.ToLowerInvariant(), keyword.ToLowerInvariant());
        }

        return text.Words().Any(word => word.Equals(keyword, (StringComparison)sc));
    }

    //check if a string contains a keyword (eg. "my nana likes bananas" - if we searched for nana, it would match the first nana, but not bananas)
    //I can already think of better ways to do this, but this worked for my needs in my language
    public static bool _ContainsKeyword(this string text, string keyword)
    {
        char? previous;
        char? next;

        if (text == keyword) { return true; }

        List<int> occurrences = text.AllIndexesOf(keyword);
        foreach (int occurrence in occurrences)
        {
            previous = null;
            next = null;
            if ((occurrence) > 0)
            {
                previous = text[occurrence - 1];
            }
            if ((occurrence + keyword.Length) < text.Length)
            {
                next = text[occurrence + keyword.Length];
            }
            if ((previous == null || !char.IsLetterOrDigit((char)previous)) &&
                (next == null || !char.IsLetterOrDigit((char)next)))
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Convert string to title CASE -> Convert String To Title Case
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string ToTitleCase(this string text)
    {
        char previous = ' ';
        StringBuilder newStringBuilder = new StringBuilder();

        foreach (char c in text)
        {
            char newC;
            if (char.IsLetter(previous))
            {
                newC = char.ToLower(c);
            }
            else
            {
                newC = char.ToUpper(c);
            }
            newStringBuilder.Append(newC);
            previous = c;
        }

        return newStringBuilder.ToString();
    }

    /// <summary>
    /// Check if the string only contains letters (abcd...)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool IsLetter(this string text)
    {
        if (text.Length == 0) { return false; }
        foreach (char c in text)
        {
            if (!char.IsLetter(c))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if the string only contains digits (1234...)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool IsDigit(this string text)
    {
        if (text.Length == 0) { return false; }
        foreach (char c in text)
        {
            if (!char.IsDigit(c))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if the string is alphanumeric (abcd...1234...)
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool IsLetterOrDigit(this string text)
    {
        if (text.Length == 0) { return false; }
        foreach (char c in text)
        {
            if (!char.IsLetterOrDigit(c))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if the string is alpha (abcd...) or contains a character in the specified character set
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool IsLetterOr(this string text, string characterSet)
    {
        if (text.Length == 0) { return false; }
        foreach (char c in text)
        {
            //if it isn't a letter, and its not in the character set then
            if (!char.IsLetter(c) && !characterSet.Contains(c))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if the string is numerical (01234...) or contains a character in the specified character set
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool IsDigitOr(this string text, string characterSet)
    {
        if (text.Length == 0) { return false; }
        foreach (char c in text)
        {
            if (!char.IsDigit(c) && !characterSet.Contains(c))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if the string is alphanumeric (abcd...1234...) or contains a character in the specified character set
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static bool IsLetterOrDigitOr(this string text, string characterSet)
    {
        if (text.Length == 0) { return false; }
        foreach (char c in text)
        {
            if (!char.IsLetterOrDigit(c) && !characterSet.Contains(c))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Count how many times a char occurs in the string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>

    public static int Occurrences(this string text, char match)
    {
        if (text.Length == 0) { return 0; }
        int count = 0;
        foreach (char c in text)
        {
            if (c == match)
            {
                count += 1;
            }
        }
        return count;
    }

    /// <summary>
    /// Count how many times a substring occurs in the string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static int Occurrences(this string text, string match, StringComparison? sc = null)
    {
        sc ??= DefaultComparisonType;
        if (text.Length == 0) { return 0; }
        if (match.Length == 0) { return 0; }
        int count = 0;
        for (int i = 0; i < text.Length - match.Length + 1; i++)
        {
            if (text.Substring(i, match.Length).Equals(match, (StringComparison)sc))
            {
                count += 1;
                i += (match.Length - 1);
            }
        }
        return count;
    }

    /// <summary>
    /// Remove all occurrences of a substring in a string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>

    public static string Replace(this string text, string toRemove)
    {
        return text.Replace(toRemove, "");
    }

    /// <summary>
    /// Remove all occurrences of a character in a string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string Replace(this string text, char toRemove)
    {
        return text.Replace(toRemove.ToString(), "");
    }

    /// <summary>
    /// check if a character occurs in the string
    /// (Extension method provided by DanStringExtensions)
    /// </summary>

    public static bool Contains(this string text, char c)
    {
        return text.Any(a => a == c);
    }

    /// <summary>
    /// Converts a string to a SecureString
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static SecureString ToSecureString(this string password)
    {
        if (password == null)
        {
            throw new ArgumentNullException("password");
        }

        var securePassword = new SecureString();

        foreach (char c in password)
        {
            securePassword.AppendChar(c);
        }

        securePassword.MakeReadOnly();
        return securePassword;
    }

    private static readonly Random Rng = new();

    /// <summary>
    /// Simple Password generator using the provided characters
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string GeneratePassword(int length, string validCharacters = Sets.AlphaNumeric)
    {
        StringBuilder res = new();

        while (0 < length--)
        {
            res.Append(validCharacters[Rng.Next(validCharacters.Length)]);
        }
        return res.ToString();
    }

    /// <summary>
    /// Fuzzy matching replaces passwords with ****** characters in text
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string HidePassword(this string text)
    {
        if (_HidePassword(ref text, "password"))
        {
            return text;
        }
        if (_HidePassword(ref text, "pass"))
        {
            return text;
        }

        if (_HidePassword(ref text, "pwd"))
        {
            return text;
        }
        return text;
    }

    private static bool _HidePassword(ref string text, string phrase)
    {
        int loc = text.ToLower().IndexOf(phrase, StringComparison.Ordinal);
        if (loc != -1)
        {
            string newText = text.Left(loc + phrase.Length) + " ******";
            text = text.TrimLeft(loc + phrase.Length + 5);
            foreach (char c in ",. ;")
            {
                int loc2 = text.ToLower().IndexOf(c);
                if (loc2 != -1)
                {
                    newText += text.TrimLeft(loc2);
                }
            }
            text = newText;
            return true;
        }
        return false;
    }

	public static string CamelCaseToSpaced(this string camelCase)
    {
        StringBuilder sb = new ();
        for (int index = 0; index < camelCase.Length; index++)
        {
            char c = camelCase[index];
            if (index >= 1 && char.IsUpper(c))
            {
                sb.Append(' ');
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Removes Diacritics (signs above letters such as an accent) from characters in a string. eg. ­Ê -> E
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    public static string RemoveDiacritics(this string s)
    {
        StringBuilder text = new StringBuilder();


        foreach (char c in s)
        {
            text.Append(c.RemoveDiacritics());
        }
        return text.ToString();
    }

    /// <summary>
    /// Remove the diacritics (signs above letters such as an accent) from a character. eg. ­Ê -> E
    /// (Extension method provided by DanStringExtensions)
    /// </summary>
    private static string RemoveDiacritics(this char c)
    {
        switch (c)
        {
            case 'ä':
            case 'æ':
            case 'ǽ':
                return "ae";
            case 'ö':
            case 'œ':
                return "oe";
            case 'À':
            case 'Á':
            case 'Â':
            case 'Ã':
            case 'Ä':
            case 'Å':
            case 'Ǻ':
            case 'Ā':
            case 'Ă':
            case 'Ą':
            case 'Ǎ':
            case 'Α':
            case 'Ά':
            case 'Ả':
            case 'Ạ':
            case 'Ầ':
            case 'Ẫ':
            case 'Ẩ':
            case 'Ậ':
            case 'Ằ':
            case 'Ắ':
            case 'Ẵ':
            case 'Ẳ':
            case 'Ặ':
            case 'А':
                return "A";
            case 'à':
            case 'á':
            case 'â':
            case 'ã':
            case 'å':
            case 'ǻ':
            case 'ā':
            case 'ă':
            case 'ą':
            case 'ǎ':
            case 'ª':
            case 'α':
            case 'ά':
            case 'ả':
            case 'ạ':
            case 'ầ':
            case 'ấ':
            case 'ẫ':
            case 'ẩ':
            case 'ậ':
            case 'ằ':
            case 'ắ':
            case 'ẵ':
            case 'ẳ':
            case 'ặ':
            case 'а':
                return "a";
            case 'Б':
                return "B";
            case 'б':
                return "b";
            case 'Ç':
            case 'Ć':
            case 'Ĉ':
            case 'Ċ':
            case 'Č':
                return "C";
            case 'ç':
            case 'ć':
            case 'ĉ':
            case 'ċ':
            case 'č':
                return "c";
            case 'Д':
                return "D";
            case 'д':
                return "d";
            case 'Ð':
            case 'Ď':
            case 'Đ':
            case 'Δ':
                return "Dj";
            case 'ð':
            case 'ď':
            case 'đ':
            case 'δ':
                return "dj";
            case 'È':
            case 'É':
            case 'Ê':
            case 'Ë':
            case 'Ē':
            case 'Ĕ':
            case 'Ė':
            case 'Ę':
            case 'Ě':
            case 'Ε':
            case 'Έ':
            case 'Ẽ':
            case 'Ẻ':
            case 'Ẹ':
            case 'Ề':
            case 'Ế':
            case 'Ễ':
            case 'Ể':
            case 'Ệ':
            case 'Е':
            case 'Э':
                return "E";
            case 'è':
            case 'é':
            case 'ê':
            case 'ë':
            case 'ē':
            case 'ĕ':
            case 'ė':
            case 'ę':
            case 'ě':
            case 'έ':
            case 'ε':
            case 'ẽ':
            case 'ẻ':
            case 'ẹ':
            case 'ề':
            case 'ế':
            case 'ễ':
            case 'ể':
            case 'ệ':
            case 'е':
            case 'э':
                return "e";
            case 'Ф':
                return "F";
            case 'ф':
                return "f";
            case 'Ĝ':
            case 'Ğ':
            case 'Ġ':
            case 'Ģ':
            case 'Γ':
            case 'Г':
            case 'Ґ':
                return "G";
            case 'ĝ':
            case 'ğ':
            case 'ġ':
            case 'ģ':
            case 'γ':
            case 'г':
            case 'ґ':
                return "g";
            case 'Ĥ':
            case 'Ħ':
                return "H";
            case 'ĥ':
            case 'ħ':
                return "h";
            case 'Ì':
            case 'Í':
            case 'Î':
            case 'Ï':
            case 'Ĩ':
            case 'Ī':
            case 'Ĭ':
            case 'Ǐ':
            case 'Į':
            case 'İ':
            case 'Η':
            case 'Ή':
            case 'Ί':
            case 'Ι':
            case 'Ϊ':
            case 'Ỉ':
            case 'Ị':
            case 'И':
            case 'Ы':
                return "I";
            case 'ì':
            case 'í':
            case 'î':
            case 'ï':
            case 'ĩ':
            case 'ī':
            case 'ĭ':
            case 'ǐ':
            case 'į':
            case 'ı':
            case 'η':
            case 'ή':
            case 'ί':
            case 'ι':
            case 'ϊ':
            case 'ỉ':
            case 'ị':
            case 'и':
            case 'ы':
            case 'ї':
                return "i";
            case 'Ĵ':
                return "J";
            case 'ĵ':
                return "j";
            case 'Ķ':
            case 'Κ':
            case 'К':
                return "K";
            case 'ķ':
            case 'κ':
            case 'к':
                return "k";
            case 'Ĺ':
            case 'Ļ':
            case 'Ľ':
            case 'Ŀ':
            case 'Ł':
            case 'Λ':
            case 'Л':
                return "L";
            case 'ĺ':
            case 'ļ':
            case 'ľ':
            case 'ŀ':
            case 'ł':
            case 'λ':
            case 'л':
                return "l";
            case 'М':
                return "M";
            case 'м':
                return "m";
            case 'Ñ':
            case 'Ń':
            case 'Ņ':
            case 'Ň':
            case 'Ν':
            case 'Н':
                return "N";
            case 'ñ':
            case 'ń':
            case 'ņ':
            case 'ň':
            case 'ŉ':
            case 'ν':
            case 'н':
                return "n";
            case 'Ö':
            case 'Ò':
            case 'Ó':
            case 'Ô':
            case 'Õ':
            case 'Ō':
            case 'Ŏ':
            case 'Ǒ':
            case 'Ő':
            case 'Ơ':
            case 'Ø':
            case 'Ǿ':
            case 'Ο':
            case 'Ό':
            case 'Ω':
            case 'Ώ':
            case 'Ỏ':
            case 'Ọ':
            case 'Ồ':
            case 'Ố':
            case 'Ỗ':
            case 'Ổ':
            case 'Ộ':
            case 'Ờ':
            case 'Ớ':
            case 'Ỡ':
            case 'Ở':
            case 'Ợ':
            case 'О':
                return "O";
            case 'ò':
            case 'ó':
            case 'ô':
            case 'õ':
            case 'ō':
            case 'ŏ':
            case 'ǒ':
            case 'ő':
            case 'ơ':
            case 'ø':
            case 'ǿ':
            case 'º':
            case 'ο':
            case 'ό':
            case 'ω':
            case 'ώ':
            case 'ỏ':
            case 'ọ':
            case 'ồ':
            case 'ố':
            case 'ỗ':
            case 'ổ':
            case 'ộ':
            case 'ờ':
            case 'ớ':
            case 'ỡ':
            case 'ở':
            case 'ợ':
            case 'о':
                return "o";
            case 'П':
                return "P";
            case 'п':
                return "p";
            case 'Ŕ':
            case 'Ŗ':
            case 'Ř':
            case 'Ρ':
            case 'Р':
                return "R";
            case 'ŕ':
            case 'ŗ':
            case 'ř':
            case 'ρ':
            case 'р':
                return "r";
            case 'Ś':
            case 'Ŝ':
            case 'Ş':
            case 'Ș':
            case 'Š':
            case 'Σ':
            case 'С':
                return "S";
            case 'ś':
            case 'ŝ':
            case 'ş':
            case 'ș':
            case 'š':
            case 'ſ':
            case 'σ':
            case 'ς':
            case 'с':
                return "s";
            case 'Ț':
            case 'Ţ':
            case 'Ť':
            case 'Ŧ':
            case 'τ':
            case 'Т':
                return "T";
            case 'ț':
            case 'ţ':
            case 'ť':
            case 'ŧ':
            case 'т':
                return "t";
            case 'Ü':
            case 'Ù':
            case 'Ú':
            case 'Û':
            case 'Ũ':
            case 'Ū':
            case 'Ŭ':
            case 'Ů':
            case 'Ű':
            case 'Ų':
            case 'Ư':
            case 'Ǔ':
            case 'Ǖ':
            case 'Ǘ':
            case 'Ǚ':
            case 'Ǜ':
            case 'Ủ':
            case 'Ụ':
            case 'Ừ':
            case 'Ứ':
            case 'Ữ':
            case 'Ử':
            case 'Ự':
            case 'У':
                return "U";
            case 'ü':
            case 'ù':
            case 'ú':
            case 'û':
            case 'ũ':
            case 'ū':
            case 'ŭ':
            case 'ů':
            case 'ű':
            case 'ų':
            case 'ư':
            case 'ǔ':
            case 'ǖ':
            case 'ǘ':
            case 'ǚ':
            case 'ǜ':
            case 'υ':
            case 'ύ':
            case 'ϋ':
            case 'ủ':
            case 'ụ':
            case 'ừ':
            case 'ứ':
            case 'ữ':
            case 'ử':
            case 'ự':
            case 'у':
                return "u";
            case 'Ý':
            case 'Ÿ':
            case 'Ŷ':
            case 'Υ':
            case 'Ύ':
            case 'Ϋ':
            case 'Ỳ':
            case 'Ỹ':
            case 'Ỷ':
            case 'Ỵ':
            case 'Й':
                return "Y";
            case 'ý':
            case 'ÿ':
            case 'ŷ':
            case 'ỳ':
            case 'ỹ':
            case 'ỷ':
            case 'ỵ':
            case 'й':
                return "y";
            case 'В':
                return "V";
            case 'в':
                return "v";
            case 'Ŵ':
                return "W";
            case 'ŵ':
                return "w";
            case 'Ź':
            case 'Ż':
            case 'Ž':
            case 'Ζ':
            case 'З':
                return "Z";
            case 'ź':
            case 'ż':
            case 'ž':
            case 'ζ':
            case 'з':
                return "z";
            case 'Æ':
            case 'Ǽ':
                return "AE";
            case 'ß':
                return "ss";
            case 'Ĳ':
                return "IJ";
            case 'ĳ':
                return "ij";
            case 'Œ':
                return "OE";
            case 'ƒ':
                return "f";
            case 'ξ':
                return "ks";
            case 'π':
                return "p";
            case 'β':
                return "v";
            case 'μ':
                return "m";
            case 'ψ':
                return "ps";
            case 'Ё':
                return "Yo";
            case 'ё':
                return "yo";
            case 'Є':
                return "Ye";
            case 'є':
                return "ye";
            case 'Ї':
                return "Yi";
            case 'Ж':
                return "Zh";
            case 'ж':
                return "zh";
            case 'Х':
                return "Kh";
            case 'х':
                return "kh";
            case 'Ц':
                return "Ts";
            case 'ц':
                return "ts";
            case 'Ч':
                return "Ch";
            case 'ч':
                return "ch";
            case 'Ш':
                return "Sh";
            case 'ш':
                return "sh";
            case 'Щ':
                return "Shch";
            case 'щ':
                return "shch";
            case 'Ъ':
            case 'ъ':
            case 'Ь':
            case 'ь':
                return "";
            case 'Ю':
                return "Yu";
            case 'ю':
                return "yu";
            case 'Я':
                return "Ya";
            case 'я':
                return "ya";
            default:
                return c.ToString();
        }
    }
}