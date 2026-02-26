using CleanArchDemo.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Application.Repositories
{
    public interface IKontoRepository
    {
        Konto Hent(int id);
        void Gem();
    }
}
