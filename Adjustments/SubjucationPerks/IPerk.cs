using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.SubjucationPerks
{
    public interface IPerk
    {
        string Explain { get; set; }
        void Activate(Pawn pawn);
        void Deactivate(Pawn pawn);
        bool CanHandle(Pawn pawn);

        string Describe(Pawn pawn);
    }
}
