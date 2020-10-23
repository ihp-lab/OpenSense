using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSense.DataStructure;

namespace OpenSense.GazeToDisplayConverter {
    public interface IGazeToDisplayConverter {

        string Name { get; set; }

        (double RSquaredX, double RSquaredY) Train(IList<Record> data);

        GazeToDisplayConverterParameters Save();

        void Load(GazeToDisplayConverterParameters param);

        (double CoordinateX, double CoordinateY) Predict(HeadPoseAndGaze data);
    }

    [Serializable]
    public abstract class GazeToDisplayConverterParameters {

        public virtual Type ConverterType { get; set; }
    }

    public static class GazeToDisplayConverterHelper {

        public static IGazeToDisplayConverter Load(string filename) {
            var path = filename;
            var json = File.ReadAllText(path);
            var jsonObj = JObject.Parse(json);
            var converterTypeString = jsonObj[nameof(GazeToDisplayConverterParameters.ConverterType)].Value<string>();
            var converterType = Type.GetType(converterTypeString);
            IGazeToDisplayConverter converter;
            switch (converterType?.Name) {
                case nameof(TwoStageConverter):
                    var twoStage = new TwoStageConverter();
                    var twoStageParam = JsonConvert.DeserializeObject<TwoStageConverterParameters>(json);
                    twoStage.Load(twoStageParam);
                    converter = twoStage;
                    break;
                case nameof(EndToEndConverter):
                    var endToEnd = new EndToEndConverter();
                    var endToEndParam = JsonConvert.DeserializeObject<EndToEndConverterParameters>(json);
                    endToEnd.Load(endToEndParam);
                    converter = endToEnd;
                    break;
                case null:
                    throw new NotSupportedException();
                default:
                    throw new InvalidOperationException();
            }
            return converter;
        }
    }

}
