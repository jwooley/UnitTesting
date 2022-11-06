using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Pages.LoanProspects
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private ApplicationDbContext _context;
        private ILogger _logger;
        private IEmailSender _emailSender;

        public CreateModel(ApplicationDbContext context,
            ILogger<CreateModel> logger,
            IEmailSender emailSender)
        {
            _context = context;
            _logger = logger;
            _emailSender = emailSender;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public LoanProspect LoanProspect { get; set; } = new LoanProspect();

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

            if (await TrySaveProspect())
            {
                return RedirectToPage("./Index");
            }
            return Page();
        }

        public async Task<bool> TrySaveProspect()
        {
            if (LoanProspect.IsSave)
            {
                if (!string.IsNullOrEmpty(LoanProspect.Email))
                {
                    await _emailSender.SendEmailAsync(LoanProspect.Email, "New Loan Inquiry", $"""
                       Thank you for your application, {LoanProspect.NameFirst}. Your loan payment would be 
                       {LoanProspect.Payment} for a loan amount of {LoanProspect.LoanAmount:c2} 
                       at a rate of {LoanProspect.InterestRate}%
                       """);
                }
                _context.LoanProspect?.Add(LoanProspect);
                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error saving", ex);
                    Confirmation = "Error saving";
                }
            }
            return false;
        }
    }
}
