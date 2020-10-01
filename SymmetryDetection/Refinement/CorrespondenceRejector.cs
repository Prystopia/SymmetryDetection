using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SymmetryDetection.Refinement
{
    public class CorrespondenceRejector
    {
        public List<Correspondence> GetRemainingCorrespondences(List<Correspondence> original)
        {
            List<Correspondence> remaining = new List<Correspondence>();

            //sort by Match Index and Distance
            original.Sort(new CorrespondenceSorter());

            Guid indexLast = Guid.Empty;

            foreach(var correspondence in original)
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

    public class CorrespondenceSorter : IComparer<Correspondence>
    {
        public int Compare([AllowNull] Correspondence x, [AllowNull] Correspondence y)
        {
            if(x.CorrespondingPoint.Id.CompareTo(y.CorrespondingPoint.Id) == 0)
            {
                return 1;
            }
            else if(x.CorrespondingPoint.Id == y.CorrespondingPoint.Id && x.Distance < y.Distance)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

}
