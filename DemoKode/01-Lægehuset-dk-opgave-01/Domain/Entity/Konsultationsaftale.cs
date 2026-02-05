namespace Domain.Entity
{
    public class Konsultationsaftale
    {
        public Konsultationstype Konsultationstype { get; private set; }
        public Læge Læge { get; private set; }
        public Patient Patient { get; init; }
        public DateTime StartTid { get; private set; }
        public TimeSpan Varighed { get; private set; }

        public Konsultationsaftale(Konsultationstype konsultationstype,
            Læge læge, Patient patient, DateTime startTid)
        {
            // Pre-condition checks
            Validatate(startTid);

            // Action
            Konsultationstype = konsultationstype;
            Læge = læge;
            Patient = patient;
            StartTid = startTid;

            Varighed = konsultationstype.Varighed;
        }

        public void UdskiftKonsultationsTypen(Konsultationstype konsultationstype)
        {
            Konsultationstype = konsultationstype;
            Varighed = konsultationstype.Varighed;

        }

        private void Validatate(DateTime startTid)
        {
            if(startTid < DateTime.Now) 
            {
                throw new ArgumentException("StartTid cannot be in the past.");
            }
        }
    }
}
