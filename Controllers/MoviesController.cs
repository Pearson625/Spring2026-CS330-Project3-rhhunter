using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Spring2026_CS330_Project3_rhhunter.Data;
using Spring2026_CS330_Project3_rhhunter.Models;
using Spring2026_CS330_Project3_rhhunter.Models.ViewModels;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using VaderSharp2;
using Azure.AI.OpenAI;

namespace Spring2026_CS330_Project3_rhhunter.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public MoviesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Poster(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var movie = await _context.Movie.FirstOrDefaultAsync(s => s.Id == id);
            if (movie == null || movie.Poster == null)
            {
                return NotFound();
            }
            return File(movie.Poster, "image/jpg");
        }

        private async Task<List<ReviewContainer>> GetReviewsAsync(string movieTitle)
        {
            var endpoint = new Uri(_configuration["OpenAI:Endpoint"]);
            var apiKey = new ApiKeyCredential(_configuration["OpenAI:ApiKey"]);
            var deployment = _configuration["OpenAI:Deployment"];

            ChatClient client = new AzureOpenAIClient(endpoint, apiKey).GetChatClient(deployment);

            string[] personas = { "is harsh", "loves romance", "loves comedy", "loves thrillers", "loves fantasy" };

            var messages = new ChatMessage[]
            {
            new SystemChatMessage(
                $"You represent a group of {personas.Length} film critics with the following personalities: " +
                $"{string.Join(", ", personas)}. When you receive a question, respond as each member " +
                $"with each response separated by a '|', but don't indicate which member you are."),
            new UserChatMessage(
                $"How would you rate the movie '{movieTitle}' out of 10 in 150 words or less?")
            };

            ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);

            string[] reviewTexts = result.Value.Content[0].Text
                .Split('|')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Take(5)
                .ToArray();

            var analyzer = new SentimentIntensityAnalyzer();

            return reviewTexts.Select(review => new ReviewContainer
            {
                Review = review,
                Score = analyzer.PolarityScores(review).Compound
            }).ToList();
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)return NotFound();


            var actors = await _context.MovieActor
                .Include(m => m.Actor)
                .Where(m => m.MovieId == id)
                .Select(m => m.Actor!)
                .ToListAsync();

            var reviews = await GetReviewsAsync(movie.Title);

            var viewModel = new MovieDetailsViewModel()
            {
                Movie = movie,
                Actors = actors,
                Reviews = reviews,
                AverageSentiment = reviews.Average(r => r.Score)
            };


            return View(viewModel);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IMDB,Genre,ReleaseYear")] Movie movie, IFormFile? Poster)
        {
            if (ModelState.IsValid)
            {
                if (Poster != null && Poster.Length > 0)
                {
                    using var stream = new MemoryStream();
                    await Poster.CopyToAsync(stream);
                    movie.Poster = stream.ToArray();
                }
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IMDB,Genre,ReleaseYear")] Movie movie, IFormFile? Poster)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Poster != null && Poster.Length > 0)
                    {
                        using var stream = new MemoryStream();
                        await Poster.CopyToAsync(stream);
                        movie.Poster = stream.ToArray();
                    }
                    else
                    {
                        var existing = await _context.Movie.AsNoTracking()
                            .FirstOrDefaultAsync(m => m.Id == id);
                        movie.Poster = existing?.Poster;
                    }
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
