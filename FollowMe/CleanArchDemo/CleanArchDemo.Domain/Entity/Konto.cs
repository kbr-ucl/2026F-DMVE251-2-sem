using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchDemo.Domain.Entity
{
    public class Konto
    {
        public int Id { get; private set; }
        public double Saldo { get; private set; }


        public void Hæv(double amount)
        {
            if (Saldo - amount < 0) throw new Exception("Ingen dækning");
            Saldo -= amount;
        }

    }
}
