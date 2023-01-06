using System.Collections.Generic;
using System.Numerics;
using OpenSense.Component.OpenFace.Common;

namespace OpenSense.Component.EyePointOfInterest.Common {
    public interface IPoiOnDisplayEstimator {

        string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>R-Square values for X and Y</returns>
        Vector2 Train(IList<GazeToDisplayCoordinateMappingRecord> data);

        PoiOnDisplayEstimatorConfiguration Save();

        Vector2 Predict(PoseAndEyeAndFace data);
    }

    

}
