namespace LibraryWeb.Models
{
    public class Employee : User
    {
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Payment> PaymentsHandled { get;  set; } = new List<Payment>();

    }
}
