using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages_GameState
{
    public class IndexModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public IndexModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        public IList<GameState> GameState { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.GameStates != null)
            {
                GameState = await _context.GameStates.OrderByDescending(g => g).ToListAsync();
            }
        }
    }
}
