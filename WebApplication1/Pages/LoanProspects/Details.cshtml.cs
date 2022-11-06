using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Pages.LoanProspects
{
    public class DetailsModel : PageModel
    {
        private readonly WebApplication1.Data.ApplicationDbContext _context;

        public DetailsModel(WebApplication1.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public LoanProspect LoanProspect { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.LoanProspect == null)
            {
                return NotFound();
            }

            var loanprospect = await _context.LoanProspect.FirstOrDefaultAsync(m => m.Id == id);
            if (loanprospect == null)
            {
                return NotFound();
            }
            else
            {
                LoanProspect = loanprospect;
            }
            return Page();
        }
    }
}
