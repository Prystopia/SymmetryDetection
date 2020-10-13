using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SymmetryDetection.Helpers
{
    public class MathsHelpers
    {
        public static float PointToPointDistance(Vector3 point1, Vector3 point2)
        {
            return Vector3.Distance(point1, point2);
        }

        public static float PointToPlaneSignedDistance(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            Vector3 planeToPointVector = point - planePoint;
            return Vector3.Dot(planeToPointVector, planeNormal);
        }
        public static float Median(IEnumerable<float> original)
        {
            float val = 0;
            int count = original.Count();
            var ordered = original.OrderBy(x => x);
            var middle = count / 2f;
            var middlePoint = (int)Math.Floor(middle);
            if (count % 2 == 0)
            {
                var val1 = ordered.Skip(middlePoint - 1).First();
                var val2 = ordered.Skip(middlePoint).First();
                val = (val1 + val2) / 2;
            }
            else
            {
                val = original.OrderBy(x => x).Skip(middlePoint).First();
            }

            return val;
        }
    }
}
