namespace LibraryWeb.Models
{
    public class Reader : User
    {
        public virtual Membership? Membership { get; set; } // опційне членство
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
