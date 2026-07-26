// Copyright 2023 AxonGenesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System.IO;
using Debug = UnityEngine.Debug;

namespace AxonGenesis
{
    public class VideoQueueItem
    {
        public bool Aborted = false;
        public bool ShowProcessWindow = false;
        public string Metadata = null;
        public string Command = "";

        public VideoEncoding Encoding = null;

        public string InputPath = null;
        public string OutputPath = null;
        public string OutputName = null;
        public bool Overwrite = true;
        public float Framerate = 30;
        public int StartingFrameNumber = 0;
        public bool OutputAudio = false;
        public float AudioStartTime = 0;
        public string AudioPath = null;

        public string OutputFilepath => OutputPath == null ? OutputName : OutputPath + OutputName;

        public void BuildCommand()
        {
            if (Encoding == null) {
                Debug.LogError($"Video Queue Item is missing encoding settings {OutputName}");
                return;
            }

            if (string.IsNullOrEmpty(Path.GetExtension(OutputName))) {
                OutputName += Encoding.Extension;
            }

            Command = $"-r {Framerate} -start_number {StartingFrameNumber} -i \"{InputPath}\" ";
            if (OutputAudio) Command += $" -i \"{AudioPath}\" ";

            Command += $"-c:v {Encoding.Codec} ";
            if (OutputAudio) Command += $"-c:a {Encoding.AudioCodec} ";

            if (!string.IsNullOrEmpty(Encoding.PixelFormat)) {
                Command += $"-pix_fmt {Encoding.PixelFormat} ";
            }
            if (Encoding.CRF >= 0) {
                Command += $"-crf {Encoding.CRF} ";
            }
            if (!string.IsNullOrEmpty(Encoding.Preset)) {
                Command += $"-preset {Encoding.Preset} ";
            }
            if (!string.IsNullOrEmpty(Encoding.Tune)) {
                Command += $"-tune {Encoding.Tune} ";
            }
            if (OutputAudio) {
                Command += $" -ss {AudioStartTime}  -shortest ";
            }
            if (!string.IsNullOrEmpty(Metadata)) {
                Command += Metadata;
            }
            if (!string.IsNullOrEmpty(Encoding.ExtraFlags)) {
                Command += Encoding.ExtraFlags;
            }
            if (Overwrite) {
                Command += " -y ";
            }
            Command += $" \"{OutputFilepath}\"";
        }

        public void Prepare()
        {
            if (Overwrite && File.Exists(OutputFilepath)) {
                File.Delete(OutputFilepath);
            }
            BuildCommand();
        }
    }
}//AxonGenesis
#endif