using CleanArch.Application.Repositories;
using CleanArchDemo.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchDemo.Infrastructur.Repositories
{
    public class KontoRepository : IKontoRepository
    {
        void IKontoRepository.Gem()
        {
            throw new NotImplementedException();
        }

        Konto IKontoRepository.Hent(int id)
        {
            return null;
        }
    }
}
