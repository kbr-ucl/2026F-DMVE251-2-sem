using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public interface ITextReader
    {
        string[] IndlæsTekst(string filNavn);
    }



    public class TekstStatistikKalkulatorNonSolid
    {
        private ITextReader _textReader;

        public TekstStatistikKalkulatorNonSolid(ITextReader textReader)
        {
            _textReader = textReader;
        }

        public TekstStatistik BeregnTekstStatistik(string filNavn)
        {
            // Indlæs fil
            string[] lines = _textReader.IndlæsTekst(filNavn); //File.ReadAllLines(fileNavn);

            // Beregn statistik
            int antalLinjer = lines.Length;
            int antalAnslag = 0;
            int antalOrd = 0;

            foreach (var line in lines)
            {
                antalAnslag += line.Length;
                antalOrd += line.Split(' ').Length;
            }

            // returner statistik
            return new TekstStatistik(antalAnslag, antalOrd, antalLinjer);
        }
    }

    public record TekstStatistik(int AntalAnslag, int AntalOrd, int AntalLinjer);
}
