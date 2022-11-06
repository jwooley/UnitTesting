using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Pages.LoanProspects
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private ApplicationDbContext _context;
        private ILogger _logger;

        public CreateModel(ApplicationDbContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public LoanProspect LoanProspect { get; set; }


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
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

            ViewData["confirmation"] = $"Loan Payment for {LoanProspect.NameFirst} is {LoanProspect.Payment:c2}";

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
                using var context = new Data.ApplicationDbContext();
                context.LoanProspect.Add(LoanProspect);
                try
                {
                   await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error saving", ex);
                    ViewData["confirmation"] = "Error saving";
                    return Page();
                }

                return RedirectToPage("./Index");
            }
            return Page();
        }
    }
}
