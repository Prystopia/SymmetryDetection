using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Interfaces
{
    public interface ICloneable<T>
    {
        T Clone();
    }
}
