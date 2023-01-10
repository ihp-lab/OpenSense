using System.Collections.Generic;
using System.Numerics;
using OpenSense.Components.OpenFace;

namespace OpenSense.Components.EyePointOfInterest {
    public interface IPoiOnDisplayEstimator {

        string Name { get; set; }

        /// <returns>R-Square values for X and Y</returns>
        Vector2 Train(IList<GazeToDisplayCoordinateMappingRecord> data);

        PoiOnDisplayEstimatorConfiguration Save();

        Vector2 Predict(PoseAndEyeAndFace data);
    }

    

}
