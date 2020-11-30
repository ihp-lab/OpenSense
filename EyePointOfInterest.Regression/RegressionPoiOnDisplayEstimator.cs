using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Numerics;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using OpenSense.Component.EyePointOfInterest.Common;
using OpenSense.Component.Head.Common;

namespace OpenSense.Component.EyePointOfInterest.Regression {
    [Export(typeof(IPoiOnDisplayEstimator))]
    public class RegressionPoiOnDisplayEstimator : IPoiOnDisplayEstimator {
        private const string FEATURE_COLUMN_NAME = nameof(RegressionRecord.Cause);

        public string Name { get; set; } = "End-to-end Regression";

        private MLContext mlContext;
        private IDataView dataView;
        private string labelX;
        private string labelY;
        private RegressionPredictionTransformer<FastTreeRegressionModelParameters> estimatorX;
        private RegressionPredictionTransformer<FastTreeRegressionModelParameters> estimatorY;
        private PredictionEngine<RegressionRecord, RegressionPrediction> predictorX;
        private PredictionEngine<RegressionRecord, RegressionPrediction> predictorY;

        public RegressionPoiOnDisplayEstimator() { }

        public RegressionPoiOnDisplayEstimator(RegressionPoiOnDisplayEstimatorConfiguration data) {
            mlContext = new MLContext(seed: 0);
            using (var s = new MemoryStream()) {
                s.Write(data.PredictorX, 0, data.PredictorX.Length);
                estimatorX = (RegressionPredictionTransformer<FastTreeRegressionModelParameters>)mlContext.Model.Load(s, out _);
            }
            using (var s = new MemoryStream()) {
                s.Write(data.PredictorY, 0, data.PredictorY.Length);
                estimatorY = (RegressionPredictionTransformer<FastTreeRegressionModelParameters>)mlContext.Model.Load(s, out _);
            }
            GenPredictors();
        }

        private void GenPredictors() {
            predictorX = mlContext.Model.CreatePredictionEngine<RegressionRecord, RegressionPrediction>(estimatorX);
            predictorY = mlContext.Model.CreatePredictionEngine<RegressionRecord, RegressionPrediction>(estimatorY);
        }

        public Vector2 Train(IList<GazeToDisplayCoordinateMappingRecord> data) {
            mlContext = new MLContext(seed: 0);
            var train = data.Select(r => new RegressionRecord(r));
            dataView = mlContext.Data.LoadFromEnumerable(train);

            labelX = nameof(RegressionRecord.DisplayX);
            var pipelineX = mlContext.Regression.Trainers.FastTree(labelX, FEATURE_COLUMN_NAME);
            estimatorX = pipelineX.Fit(dataView);
            labelY = nameof(RegressionRecord.DisplayY);
            var pipelineY = mlContext.Regression.Trainers.FastTree(labelY, FEATURE_COLUMN_NAME);
            estimatorY = pipelineY.Fit(dataView);
            GenPredictors();

            var transX = estimatorX.Transform(dataView);
            var evalX = mlContext.Regression.Evaluate(transX, labelX);
            var transY = estimatorY.Transform(dataView);
            var evalY = mlContext.Regression.Evaluate(transY, labelY);
            return new Vector2((float)evalX.RSquared, (float)evalY.RSquared);
        }

        public Vector2 Predict(HeadPoseAndGaze data) {
            var record = new RegressionRecord(data);
            var predX = predictorX.Predict(record);
            var predY = predictorY.Predict(record);
            return new Vector2(predX.Value, predY.Value);
        }

        public PoiOnDisplayEstimatorConfiguration Save() {
            var param = new RegressionPoiOnDisplayEstimatorConfiguration();
            using (var s = new MemoryStream()) {
                mlContext.Model.Save(estimatorX, dataView.Schema, s);
                param.PredictorX = s.ToArray();
            }
            using (var s = new MemoryStream()) {
                mlContext.Model.Save(estimatorY, dataView.Schema, s);
                param.PredictorY = s.ToArray();
            }
            return param;
        }
    }
}
