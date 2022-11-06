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

        [BindProperty]
        public string Confirmation { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            LoanProspect.ParseName();
            LoanProspect.ComputePayment();

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
                using var context = new Data.ApplicationDbContext();
                context.LoanProspect.Add(LoanProspect);
                try
                {
                    await context.SaveChangesAsync();
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
