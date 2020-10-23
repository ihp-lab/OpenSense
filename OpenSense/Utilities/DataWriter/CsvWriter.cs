using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Psi;
using OpenSense.DataStructure;

namespace OpenSense.Utilities.DataWriter {
    public class CsvWriter : IDataWriter {

        private StreamWriter Writer;

        private bool PendingWriteHeader;

        private bool UnixTimeStamp;

        public CsvWriter(Pipeline pipeline, DataWriterConfiguration config) {
            if (!config.Enabled) {
                return;
            }
            Writer = new StreamWriter(config.Filename, config.Append, Encoding.UTF8);
            PendingWriteHeader = config.WriteHeaders;
            UnixTimeStamp = config.UnixTimeStamp;
            pipeline.PipelineCompleted += (s, e) => Close();
        }

        ~CsvWriter() {
            Dispose();
        }

        public void Dispose() {
            if (Writer is null) {
                return;
            }
            Writer.Dispose();
            Writer = null;
        }

        public void Write(object data, Envelope e) {
            if (Writer is null) {
                return;
            }
            var items = new List<string>();
            items.Add(UnixTimeStamp ? (new DateTimeOffset(e.OriginatingTime)).ToUnixTimeMilliseconds().ToString() : e.OriginatingTime.ToString());
            void writeHeaders(string[] headers) {
                Writer.WriteLine(string.Join(",", headers));
                PendingWriteHeader = false;
            }
            IEnumerable<string> values;
            switch (data) {
                case Emotions emotions:
                    if (PendingWriteHeader) {
                        var headers = new[] {
                            "Timestamp",
                            "Angry",
                            "Disgust",
                            "Fear",
                            "Happy",
                            "Neutral",
                            "Sad",
                            "Surprise",
                        };
                        writeHeaders(headers);
                    }
                    values = new[] { 
                        emotions.Angry, 
                        emotions.Disgust, 
                        emotions.Fear, 
                        emotions.Happy, 
                        emotions.Neutral, 
                        emotions.Sad, 
                        emotions.Surprise 
                    }.Select(val => val.ToString());
                    items.AddRange(values);
                    break;
                case HeadPoseAndGaze headPoseAndGaze:
                    if (PendingWriteHeader) {
                        var headers = new[] {
                            "Timestamp",
                            "HeadPose.Position.X",
                            "HeadPose.Position.Y",
                            "HeadPose.Position.Z",
                            "HeadPose.Angle.X",
                            "HeadPose.Angle.Y",
                            "HeadPose.Angle.Z",
                            "Gaze.GazeVector.Left.X",
                            "Gaze.GazeVector.Left.Y",
                            "Gaze.GazeVector.Left.Z",
                            "Gaze.Angle.X",
                            "Gaze.Angle.Y",
                        };
                        writeHeaders(headers);
                    }
                    values = new[] {//Not all fields are listed here
                        headPoseAndGaze.HeadPose.Position.X,
                        headPoseAndGaze.HeadPose.Position.Y,
                        headPoseAndGaze.HeadPose.Position.Z,
                        headPoseAndGaze.HeadPose.Angle.X,
                        headPoseAndGaze.HeadPose.Angle.Y,
                        headPoseAndGaze.HeadPose.Angle.Z,
                        headPoseAndGaze.Gaze.GazeVector.Left.X,
                        headPoseAndGaze.Gaze.GazeVector.Left.Y,
                        headPoseAndGaze.Gaze.GazeVector.Left.Z,
                        headPoseAndGaze.Gaze.Angle.X,
                        headPoseAndGaze.Gaze.Angle.Y,
                    }.Select(val => val.ToString());
                    items.AddRange(values);
                    break;
                default:
                    items.Add(data.ToString());
                    break;
            }
            var line = string.Join(",", items);
            Writer.WriteLine(line);
        }

        public void Close() {
            Dispose();
        }
    }
}
