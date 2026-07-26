// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Linq;
using UnityEngine;
using System.Globalization;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Global utility methods for working with strings and parsing values
    /// </summary>
    public static class StringUtil
    {
        public const float MaximumTimeValue = 1000000f;

        #region NUMBER PADDING

        public static string PadNumber(int number, int places, string negativePrefix = "-")
        {
            bool neg = number < 0;
            if (number < 0) {
                if (number == int.MinValue) {
                    number = int.MaxValue;
                }
                else {
                    number = Math.Abs(number);
                }
            }
            string str = "";

            if (places < 2) {
                str += number;
            }
            else {
                bool set = false;
                for (int n = 1; n <= places; n++) {
                    float thresh = Mathf.Pow(10f, (float)n) - 1f; // Power of 10 - 1 (ex 9, 99, 999, 9999, etc)
                    if (number <= thresh) {
                        for (int x = 0; x < places - n; x++) {
                            str += "0";
                        }
                        str += number;
                        set = true;
                        break;
                    }
                }
                if (!set) {
                    str += number;
                }
            }

            if (neg && !string.IsNullOrEmpty(negativePrefix)) str = negativePrefix + str;
            return str;
        }

        public static string PadNumber2(int num)
        {
            return PadNumber(num, 2);
        }

        public static string PadNumber3(int num)
        {
            return PadNumber(num, 3);
        }

        public static string PadNumberFormat(int value, string userFormat)
        {
            if (string.IsNullOrEmpty(userFormat)) {
                userFormat = TimeflowPreferences.Current.PadNumberFormat;
            }
            try {
                if (userFormat.Contains("{0"))
                    return string.Format(userFormat, value); // advanced
                else
                    return "_" + value.ToString(userFormat); // simple fallback
            }
            catch (Exception e) {
                Debug.LogWarning($"Invalid format: {userFormat}, error: {e.Message}");
                return value.ToString(); // fallback
            }
        }

        public static void RemoveNumbersFromNames(GameObject[] allObjects)
        {
            // Regex: Match names that end with space followed by digits (e.g., "Object 01")
            Regex trailingNumberPattern = new Regex(@"\s\d+$");

            int renameCount = 0;

            foreach (GameObject obj in allObjects) {
                string originalName = obj.name;

                // Check for match
                if (trailingNumberPattern.IsMatch(originalName)) {
                    // Replace the trailing number
                    string newName = trailingNumberPattern.Replace(originalName, "");
#if UNITY_EDITOR
                    Undo.RecordObject(obj, "Remove Trailing Numbers");
#endif
                    obj.name = newName;
                    renameCount++;
                }
            }
        }

        #endregion

        #region PARSING

        public static string BoolToString(bool value)
        {
            return value ? "1" : "0";
        }

        public static string ColorToString(Color value)
        {
            return "" + value.r + "," + value.g + "," + value.b + "," + value.a;
        }

        public static string Vector2ToString(Vector2 value)
        {
            return "" + value.x + "," + value.y;
        }

        public static string Vector3ToString(Vector3 value)
        {
            return "" + value.x + "," + value.y + "," + value.z;
        }

        public static string Vector4ToString(Vector4 value)
        {
            return "" + value.x + "," + value.y + "," + value.z + "," + value.w;
        }

        public static string RectToString(Rect value)
        {
            return "" + value.x + "," + value.y + "," + value.width + "," + value.height;
        }

        public static string QuaternionToString(Quaternion value)
        {
            return "" + value.x + "," + value.y + "," + value.z + "," + value.w;
        }

        public static string BoundsToString(Bounds value)
        {
            return "" + value.center.x + "," + value.center.y + "," + value.center.z + "," + value.size.x + "," + value.size.y + "," + value.size.z;
        }

        public static string ObjectToString(UnityEngine.Object obj)
        {
            if (obj != null) {
                return "" + obj.GetInstanceID();
            }
            else {
                return "0";
            }
        }

        public static bool ParseBool(string num)
        {
            if (num == "True" || num == "1") return true;
            else
            if (num == "False" || num == "0") return false;
            else
                return ParseInt(num, 0) == 1;
        }

        public static int ParseInt(string num)
        {
            return ParseInt(num, 0);
        }

        public static int ParseInt(string num, int defaultValue)
        {
            int val = defaultValue;
            if (!int.TryParse(num, out val)) {
                val = defaultValue;
            }
            return val;
        }

        public static float ParseFloat(string num)
        {
            return ParseFloat(num.Trim(), 0.0f);
        }

        public static float ParseFloat(string num, float defaultValue)
        {
            float val = defaultValue;
            float.TryParse(num, out val);
            return val;
        }

        public static double ParseDouble(string num)
        {
            return ParseDouble(num, 0.0);
        }

        public static double ParseDouble(string num, double defaultValue)
        {
            double val = defaultValue;
            double.TryParse(num, out val);
            return val;
        }

        /// <summary>
        /// Takes an input string in the form of "1.0, 1.0, 1.0, 1.0" and converts it to a Color.
        /// </summary>
        public static Color ParseColor(string str)
        {
            Color color = Color.white;
            if (str.IndexOf(",") > -1) {
                bool alphaIsSet = false;
                string[] parts = str.Split(","[0]);
                //int count = parts.Length;
                if (0 < parts.Length) color.r = ParseFloat(parts[0]);
                if (1 < parts.Length) color.g = ParseFloat(parts[1]);
                if (2 < parts.Length) color.b = ParseFloat(parts[2]);
                if (3 < parts.Length) {
                    color.a = ParseFloat(parts[3]);
                    alphaIsSet = true;
                }

                if (!alphaIsSet) color.a = 1.0f;
            }
            return color;
        }

        /// <summary>
        /// Takes an input string in the form of "1.0, 1.0, 1.0, 1.0" and converts it to a Vector2.
        /// </summary>
        public static Vector2 ParseVector2(string str)
        {
            Vector2 v = Vector2.zero;
            if (str.IndexOf(",") > -1) {
                string[] parts = str.Split(","[0]);
                //int count = parts.Length;
                if (0 < parts.Length) v.x = ParseFloat(parts[0]);
                if (1 < parts.Length) v.y = ParseFloat(parts[1]);
            }
            return v;
        }

        /// <summary>
        /// Takes an input string in the form of "1.0, 1.0, 1.0, 1.0" and converts it to a Vector3.
        /// </summary>
        public static Vector3 ParseVector3(string str)
        {
            Vector3 v = Vector3.zero;
            if (str.IndexOf(",") > -1) {
                string[] parts = str.Split(","[0]);
                if (0 < parts.Length) v.x = ParseFloat(parts[0]);
                if (1 < parts.Length) v.y = ParseFloat(parts[1]);
                if (2 < parts.Length) v.z = ParseFloat(parts[2]);
            }
            return v;
        }

        /// <summary>
        /// Takes an input string in the form of "1.0, 1.0, 1.0, 1.0" and converts it to a Vector4.
        /// </summary>
        public static Vector4 ParseVector4(string str)
        {
            Vector4 v = Vector4.zero;
            if (str.IndexOf(",") > -1) {
                string[] parts = str.Split(","[0]);
                if (0 < parts.Length) v.x = ParseFloat(parts[0]);
                if (1 < parts.Length) v.y = ParseFloat(parts[1]);
                if (2 < parts.Length) v.z = ParseFloat(parts[2]);
                if (3 < parts.Length) v.w = ParseFloat(parts[3]);
            }
            return v;
        }

        /// <summary>
        /// Takes an input string in the form of "1.0, 1.0, 1.0, 1.0" and converts it to a Rect.
        /// </summary>
        public static Rect ParseRect(string str)
        {
            Rect v = new Rect();
            if (str.IndexOf(",") > -1) {
                string[] parts = str.Split(","[0]);
                if (0 < parts.Length) v.x = ParseFloat(parts[0]);
                if (1 < parts.Length) v.y = ParseFloat(parts[1]);
                if (2 < parts.Length) v.width = ParseFloat(parts[2]);
                if (3 < parts.Length) v.height = ParseFloat(parts[3]);
            }
            return v;
        }

        public static Quaternion ParseQuaternion(string str)
        {
            Quaternion v = Quaternion.identity;
            if (str.IndexOf(",") > -1) {
                string[] parts = str.Split(","[0]);
                if (0 < parts.Length) v.x = ParseFloat(parts[0]);
                if (1 < parts.Length) v.y = ParseFloat(parts[1]);
                if (2 < parts.Length) v.z = ParseFloat(parts[2]);
                if (3 < parts.Length) v.w = ParseFloat(parts[3]);
            }
            return v;
        }

        public static Bounds ParseBounds(string str)
        {
            Vector3 c = Vector3.zero;
            Vector3 s = Vector3.zero;
            if (str.IndexOf(",") > -1) {
                string[] parts = str.Split(","[0]);
                if (0 < parts.Length) c.x = ParseFloat(parts[0]);
                if (1 < parts.Length) c.y = ParseFloat(parts[1]);
                if (2 < parts.Length) c.z = ParseFloat(parts[2]);
                if (3 < parts.Length) s.x = ParseFloat(parts[3]);
                if (4 < parts.Length) s.y = ParseFloat(parts[4]);
                if (5 < parts.Length) s.z = ParseFloat(parts[5]);
            }
            return new Bounds(c, s);
        }

        public static UnityEngine.Object ParseObject(int instanceId)
        {
            UnityEngine.Object obj = null;
#if UNITY_EDITOR
            if (instanceId != 0) {
                obj = EditorUtility.InstanceIDToObject(instanceId);
            }
#endif
            return obj;
        }

        #endregion

        #region UTILITIES

        public static string GetSafeName(string name)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) {
                name = name.Replace(c, '_');
            }
            return name;
        }

        public static string Reverse(string s)
        {
            char[] a = s.ToCharArray();
            Array.Reverse(a);
            return new string(a);
        }

        public static string TextClean(string input)
        {
            string output = "";
            if (!string.IsNullOrEmpty(input)) {
                output = input.Trim();
                output = output.Replace("\n", "");
                output = output.Replace("\r", "");
                output = output.Replace("\t", "");

                char ch = (char)25;
                output = output.Replace("" + ch, "");
            }
            return output;
        }

        public static string ClassName(Type type)
        {
            return ClassName("" + type);
        }

        public static string ClassName(string input)
        {
            string output = "";
            if (!string.IsNullOrEmpty(input)) {
                if (input.Contains(".")) {
                    int i = input.LastIndexOf(".") + 1;
                    input = input.Substring(i);
                }

                output = TextClean(input);
            }
            return output;
        }

        public static string ToCamelCase(string input)
        {
            // Split the string by spaces
            string[] words = input.Split(' ');

            // If there's no input, return an empty string
            if (words.Length == 0) {
                return string.Empty;
            }

            // Process the first word - convert it to lowercase
            string camelCaseString = words[0].ToLower(CultureInfo.InvariantCulture);

            // Process remaining words - capitalize first letter and lowercase the rest
            for (int i = 1; i < words.Length; i++) {
                if (!string.IsNullOrEmpty(words[i])) {
                    string word = words[i].ToLower(CultureInfo.InvariantCulture);
                    camelCaseString += char.ToUpper(word[0], CultureInfo.InvariantCulture) + word.Substring(1);
                }
            }

            return camelCaseString;
        }

        public static string IncrementName(string input)
        {
            if (string.IsNullOrEmpty(input)) return " (1)";
            // Regular expression to match a number in parentheses at the end of the string
            Regex regex = new Regex(@"\((\d+)\)$");
            Match match = regex.Match(input);

            if (match.Success) {
                // Extract the number, increment it, and replace the old number in the string
                int number = int.Parse(match.Groups[1].Value);
                return regex.Replace(input, $"({number + 1})");
            }
            else {
                // Append "(1)" if no number in parentheses is found
                return $"{input} (1)";
            }
        }

        public static string RemoveEmojisAndTrim(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Remove all emoji and symbol characters (Unicode categories: So, Sk, Sm, Sc, Cf)
            var output = Regex.Replace(
                input,
                @"([\p{So}\p{Sk}\p{Sm}\p{Sc}\p{Cf}])",
                string.Empty
            );

            // Remove extra spaces before/after slashes and trim
            output = string.Join("/", output.Split('/').Select(s => s.Trim()));
            output = output.Replace("/ ", "/");

            // Remove all types of whitespace, including non-standard ones
            //output = Regex.Replace(output, @"\s+", ""); // Removes all whitespace characters
            output = ToAscii(output);
            output.Replace("?", "");
            return output.Trim();
        }


        /// <summary>
        /// Converts a string into its ASCII character representation.
        /// Non-ASCII characters will be replaced with '?'.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>A string containing only ASCII characters.</returns>
        public static string ToAscii(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var asciiBytes = System.Text.Encoding.ASCII.GetBytes(input);
            return new string(asciiBytes.Select(b => (char)b).ToArray());
        }



        #endregion

        #region TIMECODE

        public static string SecondsToTimecode(float time) { return SecondsToTimecode(time, true); }

        public static string SecondsToTimecode(float time, bool showFraction = true, bool fractionAsFrames = false, float fps = 60f, bool showDays = false)
        {
            if (time > MaximumTimeValue) {
                Debug.LogWarning($"The time value {time} exceeds the maximum allowed value {MaximumTimeValue}");
                time = MaximumTimeValue;
            }
            else
            if (time < -MaximumTimeValue) {
                Debug.LogWarning($"The time value {time} exceeds the maximum allowed value {MaximumTimeValue}");
                time = -MaximumTimeValue;
            }

            int hours = Mathf.FloorToInt(time / (60f * 60f));
            float t = time - (float)hours * 60f * 60f;
            int minutes = Mathf.FloorToInt(t / 60f);
            t = t - (float)minutes * 60f;
            int seconds = Mathf.FloorToInt(t);
            t = t - (float)seconds;
            int miliseconds = Mathf.FloorToInt(1000f * t);

            int days = 0;
            string timecode = "";
            if (showDays && hours > 24) {
                days = Mathf.FloorToInt((float)hours / 24f);
                timecode += days + "d ";
                hours = hours - (days * 24);
            }


            if (hours != 0) timecode += StringUtil.PadNumber2(hours) + ":";
            timecode += StringUtil.PadNumber2(minutes) + ":" + StringUtil.PadNumber2(seconds);
            if (showFraction) {
                if (fractionAsFrames) {
                    timecode += "." + StringUtil.PadNumber2(Mathf.RoundToInt(fps * (miliseconds / 1000f)));
                }
                else {
                    timecode += "." + StringUtil.PadNumber3(miliseconds);
                }
            }
            return timecode;
        }

        public static float TimecodeToSeconds(string time, bool showFraction = true, bool fractionAsFrames = false, float fps = 60f)
        {
            if (time.Contains(".")) time = time.Replace(".", ":");
            if (time.Contains(";")) time = time.Replace(";", ":");
            string[] parts = time.Split(new char[] { ':' });

            float seconds = 0;
            if (parts.Length == 1) {
                seconds = (float)ParseSeconds(parts[0]);
            }
            else
            if (parts.Length == 2) {
                seconds += ParseMinutes(parts[0]);
                seconds += ParseSeconds(parts[1]);
            }
            else
            if (parts.Length == 3) {
                if (showFraction) {
                    /// Note that when showFraction is on, the first part is considered minutes
                    seconds += ParseMinutes(parts[0]);
                    seconds += ParseSeconds(parts[1]);
                    seconds += ParseFraction(parts[2], fractionAsFrames, fps);
                }
                else {
                    /// And when showFraction is off the first part is treated as the hour
                    seconds += ParseHours(parts[0]);
                    seconds += ParseMinutes(parts[1]);
                    seconds += ParseSeconds(parts[2]);
                }
            }
            else
            if (parts.Length == 4) {
                seconds += ParseHours(parts[0]);
                seconds += ParseMinutes(parts[1]);
                seconds += ParseSeconds(parts[2]);
                seconds += ParseFraction(parts[3], fractionAsFrames, fps);
            }
            else {
                Debug.LogWarning("Malformed timecode:" + time);
            }
            return seconds;
        }

        private static float ParseHours(string hours)
        {
            return (float)StringUtil.ParseInt(hours) * 60f * 60f;
        }

        private static float ParseMinutes(string minutes)
        {
            return (float)StringUtil.ParseInt(minutes) * 60f;
        }

        private static float ParseSeconds(string minutes)
        {
            return (float)StringUtil.ParseInt(minutes);
        }

        private static float ParseFraction(string seconds, bool fractionAsFrames, float fps)
        {
            float fraction = StringUtil.ParseFloat(seconds);
            if (fractionAsFrames && fps != 0) {
                return fraction / fps;
            }
            else {
                return fraction / 1000f;
            }
        }

        #endregion

        #region MEASURES

        public static string SecondsToMeasures(float time, float bpm, int beatsPerBar, int beatSize)
        {
            string neg = "";
            if (time < 0) {
                neg = "-";
                time = Mathf.Abs(time);
            }
            float barDuration = bpm == 0 ? 1 : (60f / bpm) * 4f * (float)beatsPerBar / (float)beatSize;

            float barsf = (time / barDuration);
            int bars = (int)Mathf.FloorToInt(barsf);
            float barsRem = barsf - (float)bars;

            float beatsf = (float)beatsPerBar * barsRem;
            int beats = (int)Mathf.FloorToInt(beatsf);
            float beatsRem = beatsf - (float)beats;

            if (beatSize <= 0) beatSize = 1;
            float sixteenthsf = (float)(16f / (float)beatSize) * beatsRem;
            int sixteenths = (int)Mathf.FloorToInt(sixteenthsf);

            /// Add 1 since there is no 0 in the display format
            bars++;
            beats++;
            sixteenths++;

            return $"{neg}{bars}.{beats}.{sixteenths}";
        }

        public static string SecondsToMeasuresShort(float time, float bpm, int beatsPerBar, int beatSize)
        {
            string val = SecondsToMeasures(time, bpm, beatsPerBar, beatSize);
            val = val.Replace(".1.1", "");
            if (val.EndsWith(".1")) {
                val = val.Replace(".1", "");
            }
            return val;
        }

        public static float MeasuresToSeconds(string measures, float bpm, int beatsPerBar, int beatSize)
        {
            if (beatsPerBar <= 0 || beatSize <= 0 || bpm <= 0) {
                Debug.LogError($"Invalid input parameters bpm:{bpm} beatsPerBar:{beatsPerBar} beatSize:{beatSize}");
                return 0;
            }
            float t = 0f;
            float beat = (float)beatsPerBar / (float)beatSize;
            float barDuration = bpm == 0 ? 1 : (60f / bpm) * 4f * (float)beatsPerBar / (float)beatSize;
            float beatDur = barDuration / (float)beatsPerBar;
            float noteDur = beatDur * (float)((float)beatSize / 16f);

            if (!measures.Contains(".")) {
                int bars = StringUtil.ParseInt(measures) - 1;
                t = (float)bars * barDuration;
            }
            else {
                string[] parts = measures.Split(new char[] { '.' });
                int bars = StringUtil.ParseInt(parts[0]) - 1;
                t = (float)bars * barDuration;

                if (parts.Length > 1) {
                    int beats = StringUtil.ParseInt(parts[1]) - 1;
                    t += (float)beats * beatDur;
                }
                if (parts.Length > 2) {
                    int notes = StringUtil.ParseInt(parts[2]) - 1;
                    t += (float)notes * noteDur;
                }
            }
            return t;
        }

        public static string Abbreviate(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            // Split the name into words based on spaces or underscores  
            var words = name.Split(new[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);

            // Take the first letter of each word, convert to uppercase, and concatenate  
            return string.Concat(words.Select(word => char.ToUpper(word[0])));
        }


        #endregion
    }

}//AxonGenesis