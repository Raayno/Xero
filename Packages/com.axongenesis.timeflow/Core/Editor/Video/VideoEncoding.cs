// Copyright 2023 AxonGenesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System;
using UnityEngine;

namespace AxonGenesis
{
    [CreateAssetMenu(fileName = "VideoEncoding", menuName = "Timeflow/New Video Encoding")]
    [Serializable]
    public class VideoEncoding : ScriptableObject
    {
        [Tooltip("Sets the encoder to use. Check the ffmpeg documentation with your install to see available options")]
        public string Codec = "libx264";

        [Tooltip("Use a predefined preset name. Check your ffmpeg install for available presets. If invalid, encoding will not be processed.")]
        public string Preset = "";

        [Tooltip("Compression setting determining the quality, where the higher the value the more compressed it is (resulting in lower quality). " +
            "The default value is 23. Enter 0 for lossless, or enter a value of -1 to skip this setting.")]
        [Range(-1, 51)]
        public int CRF = 23;

        [Tooltip("Additional preset for different media types. Possible values include: film, animation, grain, stillimage, fastdecode, zerolatency.")]
        public string Tune = "film";

        [Tooltip("Sets the audio codec to be used.")]
        public string AudioCodec = "aac";

        [Tooltip("Sets the pixel format for the encoding. Check the ffmpeg documentation for options. Note that warnings may occur but may be ignored.")]
        public string PixelFormat = "yuv420p";

        [Tooltip("Enter any additional ffmpeg command line settings. Leave this empty to ignore.")]
        public string ExtraFlags = "";

        [Tooltip(@"Enter the file extension for the output video. Please be sure to begin with a dot (example: .mp4)")]
        public string Extension = ".mp4";
    }
}//AxonGenesis
#endif