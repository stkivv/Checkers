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
    public class DeleteModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DeleteModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
      public GameState GameState { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.GameStates == null)
            {
                return NotFound();
            }

            var gamestate = await _context.GameStates.FirstOrDefaultAsync(m => m.GameStateId == id);

            if (gamestate == null)
            {
                return NotFound();
            }
            else 
            {
                GameState = gamestate;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.GameStates == null)
            {
                return NotFound();
            }
            var gamestate = await _context.GameStates.FindAsync(id);

            if (gamestate != null)
            {
                GameState = gamestate;
                _context.GameStates.Remove(GameState);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
