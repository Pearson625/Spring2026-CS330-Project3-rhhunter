using Azure.AI.OpenAI;
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
using System.Threading.Tasks;
using VaderSharp2;


namespace Spring2026_CS330_Project3_rhhunter.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ActorsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Photo(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var actor = await _context.Actor.FirstOrDefaultAsync(s => s.Id == id);
            if (actor == null || actor.Photo == null)
            {
                return NotFound();
            }
            return File(actor.Photo, "image/jpg");
        }

        private async Task<List<TweetContainer>> GetTweetsAsync(string actorName)
        {
            var endpoint = new Uri(_configuration["OpenAI:Endpoint"]);
            var apiKey = new ApiKeyCredential(_configuration["OpenAI:ApiKey"]);
            var deployment = _configuration["OpenAI:Deployment"];

            ChatClient client = new AzureOpenAIClient(endpoint, apiKey)
                .GetChatClient(deployment);

            var messages = new ChatMessage[]
            {
            new SystemChatMessage(
                "You represent the Twitter social media platform. " +
                "Generate tweets from a variety of users with different opinions. " +
                "Separate each tweet with a '|' and do not include usernames, " +
                "numbers, or any extra text — only the tweet content."),
            new UserChatMessage(
                $"Generate exactly 10 tweets from different users about the actor {actorName}.")
            };

            ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);

            string[] tweetTexts = result.Value.Content[0].Text
                .Split('|')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Take(10)
                .ToArray();

            var analyzer = new SentimentIntensityAnalyzer();

            return tweetTexts.Select(tweet => new TweetContainer
            {
                Tweet = tweet,
                Score = analyzer.PolarityScores(tweet).Compound
            }).ToList();
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actor.FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null) return NotFound();

            var movies = await _context.MovieActor
                .Include(m => m.Movie)
                .Where(m => m.ActorId == id)
                .Select(m => m.Movie!)
                .ToListAsync();

            var tweets = await GetTweetsAsync(actor.Name);

            var viewModel = new ActorDetailsViewModel()
            {
                Actor = actor,
                Movies = movies,
                Tweets = tweets,
                AverageSentiment = tweets.Average(t => t.Score)
            };

            return View(viewModel);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMDB")] Actor actor, IFormFile? Photo)
        {
            if (ModelState.IsValid)
            {
                if (Photo != null && Photo.Length > 0)
                {
                    using var Stream = new MemoryStream();
                    await Photo.CopyToAsync(Stream);
                    actor.Photo = Stream.ToArray();
                }
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMDB")] Actor actor, IFormFile? Photo)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Photo != null && Photo.Length > 0)
                    {
                        using var Stream = new MemoryStream();
                        await Photo.CopyToAsync(Stream);
                        actor.Photo = Stream.ToArray();
                    }
                    else
                    {
                        var existing = await _context.Actor.AsNoTracking()
                            .FirstOrDefaultAsync(m => m.Id == id);
                        actor.Photo = existing?.Photo;
                    }
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }


        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
