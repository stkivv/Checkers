using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL.Db;
using Domain;
using GameBrain;

namespace WebApp.Pages_CheckersGame
{
    public class CreateModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;
        private readonly IGameRepository _repo;

        public CreateModel(DAL.Db.AppDbContext context, IGameRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        public IActionResult OnGet()
        {
            OptionsSelectList = new SelectList(_context.GameOptions, "CheckersOptionsId", "OptionsName");
            return Page();
        }

        [BindProperty]
        public CheckersGame CheckersGame { get; set; } = default!;

        public SelectList OptionsSelectList { get; set; } = default!;

        public SelectList Player1SideSelectList { get; } = new (new List<EPlayerSide>{ EPlayerSide.WhiteSide, EPlayerSide.WhiteSideAI});
        public SelectList Player2SidSelectList { get; } = new (new List<EPlayerSide>{ EPlayerSide.BlackSide, EPlayerSide.BlackSideAI});

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || _context.CheckersGames == null || CheckersGame == null)
            {
                return Page();
            }
            _repo.AddGame(CheckersGame);

            CheckersGame game = _repo.GetGame(CheckersGame.CheckersGameId)!;
            CheckersBrain brain = new CheckersBrain
            {
                Options = game.Options
            };
            brain.InitializeGame();
            game.StartedAt = DateTime.Now;

            GameState state = new GameState()
            {
                GameStateName = "Web - " + game.Player1Name + "," + game.Player2Name +
                                " - game started at: " + game.StartedAt + " - saved at: --",
                GameBoard = JsonSerializer.Serialize(brain.ConvertBoardToJaggedArray()),
                WhiteTurn = brain.WhiteTurn
            };
            game.States = new List<GameState>();
            game.States.Add(state);
            _repo.SaveChanges(game);

            return RedirectToPage("./Index");
        }
    }
}
