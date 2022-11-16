using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Pages.LoanProspects
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private ILogger _logger;
        private ApplicationDbContext _context;

        public CreateModel(ILogger<CreateModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public LoanProspect LoanProspect { get; set; }

        [BindProperty]
        public string Confirmation { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var nameParts = LoanProspect.Name.Split(" ");
            LoanProspect.NameFirst = nameParts[0];
            LoanProspect.NameLast = nameParts[1];
            LoanProspect.Payment = -1 * Microsoft.VisualBasic.Financial.Pmt(LoanProspect.InterestRate / 1200.0, LoanProspect.TermMonths, LoanProspect.LoanAmount, 0);

            Confirmation = $"Loan Payment for {LoanProspect.NameFirst} is {LoanProspect.Payment:c2}";

            if (LoanProspect.IsSave)
            {
                if (!string.IsNullOrEmpty(LoanProspect.Email))
                {
                    await new EmailSender().SendEmailAsync(LoanProspect.Email, "New Loan Inquiry", $"""
                       Thank you for your application, {LoanProspect.NameFirst}. Your loan payment would be 
                       {LoanProspect.Payment} for a loan amount of {LoanProspect.LoanAmount:c2} 
                       at a rate of {LoanProspect.InterestRate}%
                       """);
                }
                _context.LoanProspect.Add(LoanProspect);
                try
                {
                   await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error saving", ex);
                    Confirmation = "Error saving";
                    return Page();
                }

                return RedirectToPage("./Index");
            }
            return Page();
        }
    }
}
