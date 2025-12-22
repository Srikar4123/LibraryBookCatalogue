namespace MiniProject.Model
{
    public class Fines
    {
        public int Id{ get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public decimal fineAmount { get; set; }
        public bool paymentStatus { get; set; }

        public DateTime IssueDate { get; set; }  
        public DateTime? ReturnDate { get; set; }
    }
}
