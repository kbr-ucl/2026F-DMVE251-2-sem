using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Test
{
    public class TekstStatistikKalkulatorTests
    {
        [Fact]
        public void BeregnTekstStatistik_Beregner_Korrekt()
        {
            // Arrange
            string[] testText = new[] { "12345 67890" };
            var expected = new TekstStatistik(11, 2, 1);

            var textReader = new Mock<ITextReader>();
            textReader.Setup(a => a.IndlæsTekst(It.IsAny<string>())).Returns(testText);

            var sut = new TekstStatistikKalkulatorNonSolid(textReader.Object);


            // Act
            var actual = sut.BeregnTekstStatistik("dummy");

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
