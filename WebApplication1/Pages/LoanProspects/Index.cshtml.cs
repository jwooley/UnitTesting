using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Pages.LoanProspects
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<LoanProspect> LoanProspect { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.LoanProspect != null)
            {
                LoanProspect = await _context.LoanProspect.ToListAsync();
            }
        }
    }
}
