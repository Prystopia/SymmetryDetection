using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Parameters
{
    public class ReflectionalSymmetryParameters : ISymmetryParameters
    {
        /// <summary>
        //Maximum value for the occlussion score - Not used
        /// </summary>
        public float MAX_OCCLUSION_SCORE { get; set; }
        /// <summary>
        /// the minimum symmetry score averaged across all points in a cloud
        /// </summary>
        public float MIN_CLOUD_INLIER_SCORE { get; set; }
        /// <summary>
        /// The Minimum symmetry score averaged across all detected correspondences
        /// </summary>
        public float MIN_CORRESPONDANCE_INLIER_SCORE { get; set; }

        //I think these will be the main parameters which will need to change depending on the size of the model

        /// <summary>
        /// Maximum allowed distance between two points to determine if they can be a correspondence 
        /// </summary>
        public float MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE { get; set; } // Amend by the scale of the sculpture - this is for 1m
        /// <summary>
        /// Minimum allowed distance of the sum of distances between a point and it's correspondence to a symmetry plane i.e. if the original is 1m and the correspondence is 3m then we should reject the symmetry plane
        /// </summary>
        public float MIN_SYMMETRY_CORRESPONDENCE_DISTANCE { get; set; }
        /// <summary>
        /// The maximum difference allowed between the distance to the origin of a point and it's correspondence
        /// </summary>
        public float MAX_REFERENCE_POINT_DISTANCE { get; set; }

        /// <summary>
        /// Number of times we will attempt to optimse the symmetry plane using the LM algorithm (not the same as the number of iterations used in the LM algorithm)
        /// </summary>
        public int MAX_ITERATIONS { get; set; }

        /// <summary>
        /// Maxmimum allowed difference in angle between two proposed symmetry planes to determine whether the planes are similar or not
        /// </summary>
        public float MAX_NORMAL_ANGLE_DIFF { get; set; }
        /// <summary>
        /// Maximum allowed difference between two proposed symmetry planes to determine whether the planes are similar or not.
        /// </summary>
        public float MAX_DISTANCE_DIFF { get; set; }

        public ReflectionalSymmetryParameters()
        {
            this.MAX_OCCLUSION_SCORE = 0.01f;
            this.MIN_CLOUD_INLIER_SCORE = 0.5f;
            this.MIN_CORRESPONDANCE_INLIER_SCORE = 0.7f;
            this.MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE = 0.05f;
            this.MIN_SYMMETRY_CORRESPONDENCE_DISTANCE = 1;
            this.MAX_REFERENCE_POINT_DISTANCE = 0.5f;
            this.MAX_ITERATIONS = 20;
            this.MAX_NORMAL_ANGLE_DIFF = 0.12221f;
            this.MAX_DISTANCE_DIFF = 0.01f;
        }
    }
}
