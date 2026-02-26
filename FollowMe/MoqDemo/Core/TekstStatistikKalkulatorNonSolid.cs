namespace Core
{
    public class TekstStatistikKalkulatorNonSolid
    {
        public TekstStatistik BeregnTekstStatistik(string fileNavn)
        {
            // Indlæs fil
            string[] lines = File.ReadAllLines(fileNavn);

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
