using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class LoanProspect
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public string Email { get; set; }
        public double LoanAmount { get; set; }
        public double InterestRate { get; set; }
        public int TermMonths { get; set; }
        public double Payment { get; set; }

        [NotMapped]
        public bool IsSave { get; set; }
    }
}
