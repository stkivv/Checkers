using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages_CheckersOptions
{
    public class DetailsModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DetailsModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

      public CheckersOptions CheckersOptions { get; set; } = default!;
      public int? GameId { get; set; }
      public EPlayerSide? PlayerSide { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, int? gameId, EPlayerSide? playerSide)
        {
            if (id == null || _context.GameOptions == null)
            {
                return NotFound();
            }

            if (gameId != null)
            {
                GameId = gameId;
            }

            if (playerSide != null)
            {
                PlayerSide = playerSide;
            }

            var checkersoptions = await _context.GameOptions.FirstOrDefaultAsync(m => m.CheckersOptionsId == id);
            if (checkersoptions == null)
            {
                return NotFound();
            }
            else 
            {
                CheckersOptions = checkersoptions;
            }
            return Page();
        }
    }
}
