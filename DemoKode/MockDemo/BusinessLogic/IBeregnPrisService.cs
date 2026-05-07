using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic
{
    // Interfacet gør det muligt at udskifte den rigtige rabatservice med et mock i unit tests.
    public interface IBeregnRabatService
    {
        public double BeregnRabatProcent(double pris);
    }
}
