using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("TestProject1")]
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

        public void ParseName()
        {
            if (string.IsNullOrEmpty(Name))
            {
                NameFirst = string.Empty;
                NameLast = string.Empty;
            }
            else
            { 
                Span<string> nameParts = Name.Split(" ");
                if (nameParts.Length == 1)
                {
                    NameLast = Name;
                    NameFirst = string.Empty;
                }
                else
                {
                    NameFirst = nameParts[0];
                    NameLast = nameParts[^1];
                }
            }
            if (NameFirst.Contains(","))
            {
                var temp = NameLast;
                NameLast = NameFirst.Replace(",", "");
                NameFirst = temp;
            }
        }

        internal void ComputePayment()
        {
            Payment = -1 * Microsoft.VisualBasic.Financial.Pmt(InterestRate / 1200.0, TermMonths, LoanAmount, 0);
        }
    }
}
