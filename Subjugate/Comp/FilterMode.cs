using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subjugate.Comp
{
    public enum FilterMode : byte
    {
        Contains,
        StartsWith,
        EndsWith,
        Equals,
        Excludes,
    }
}
