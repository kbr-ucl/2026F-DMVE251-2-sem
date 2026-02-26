using CleanArchDemo.Domain.Entity;

namespace CleanArchDemo.Domain.Test
{
    public class KontoTest
    {
        [Fact]
        public void Hæv_ved_overtræk_throw_Exception()
        {
            //
            var sut = new Konto();

            //

            //
            Assert.Throws<Exception>(() => sut.Hæv(10));
        }
    }
}
