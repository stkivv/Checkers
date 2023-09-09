using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages_CheckersGame
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;
        private readonly IGameRepository _repo;


        public DeleteModel(DAL.Db.AppDbContext context, IGameRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        [BindProperty]
      public CheckersGame CheckersGame { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.CheckersGames == null)
            {
                return NotFound();
            }

            var checkersgame = await _context.CheckersGames.FirstOrDefaultAsync(m => m.CheckersGameId == id);

            if (checkersgame == null)
            {
                return NotFound();
            }
            else 
            {
                CheckersGame = checkersgame;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.CheckersGames == null)
            {
                return NotFound();
            }
            
            CheckersGame? game = _repo.GetGame(id);
            if (game != null)
            {
                _repo.DeleteGame(game);
            }

            return RedirectToPage("./Index");
        }
    }
}
