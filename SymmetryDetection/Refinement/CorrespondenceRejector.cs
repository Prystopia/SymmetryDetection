using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace SymmetryDetection.Refinement
{
    public class CorrespondenceRejector
    {
        public List<Correspondence> GetRemainingCorrespondences(List<Correspondence> original)
        {
            List<Correspondence> remaining = new List<Correspondence>();

            //sort by Match Index and Distance
           var sortedOriginal = original.OrderBy(c => c.CorrespondingPoint.Id).ThenBy(c => c.Distance);

            Guid indexLast = Guid.Empty;

            foreach(var correspondence in sortedOriginal)
            {
                if(correspondence.CorrespondingPoint.Id != Guid.Empty)
                {
                    if(correspondence.CorrespondingPoint.Id != indexLast)
                    {
                        remaining.Add(correspondence);
                        indexLast = correspondence.CorrespondingPoint.Id;
                    }
                }
            }

            return remaining;
        }
    }
}
