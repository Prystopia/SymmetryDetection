using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Parameters
{
    public class RotationalSymmetryParameters
    {
        public const float MAX_SYMMETRY_SCORE = 0.01f;
        public const float MAX_OCCLUSION_SCORE = 1;
        public const float MAX_PERPENDICULAR_SCORE = 0.65f;
        public const float MIN_COVERAGE_SCORE = 0.3f;
        public const float REFERENCE_MAX_FIT_ANGLE = 0.785398f; //45 degrees
    }
}
