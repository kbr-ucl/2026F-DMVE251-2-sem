using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchDemo.Facade.Commands
{
    public interface IHævBeløbUseCase
    {
        void Hæv(HævBeløbCommand command);
    }

    public record HævBeløbCommand(int KontoId, double Amount);
}
