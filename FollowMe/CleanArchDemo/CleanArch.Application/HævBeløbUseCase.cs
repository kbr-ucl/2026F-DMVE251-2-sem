using CleanArch.Application.Repositories;
using CleanArchDemo.Facade.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Application
{
    public class HævBeløbUseCase : IHævBeløbUseCase
    {
        private IKontoRepository _repo;
        public HævBeløbUseCase(IKontoRepository repo)
        {
            _repo = repo;
        }
         void IHævBeløbUseCase.Hæv(HævBeløbCommand command)

        {
            // Load
            var konto = _repo.Hent(command.KontoId);

            // Do
            konto.Hæv(command.Amount);

            // Save
            _repo.Gem();
        }
    }
}
