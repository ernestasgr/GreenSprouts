using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using test.Models;

namespace test.Controllers
{
	public class ArticleController : Controller
    {
        private readonly AppDbContext db;

		public ArticleController(AppDbContext db)
		{
			this.db = db;
		}

        [HttpGet]
        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var totalArticles = db.Articles.Count();

            // Calculate number of pages
            var totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

            // Ensure requested page is within bounds
            page = Math.Max(1, Math.Min(page, totalPages));

            // Get articles for the current page
            var articles = db.Articles
                .OrderBy(a => a.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var uniqueTopics = articles.Select(t => t.Topic).Distinct().ToList();

            // Create view model
            var model = new ArticleViewModel
            {
                Articles = articles,
                CurrentPage = page,
                TotalPages = totalPages,
                ItemsPerPage = pageSize,
                UniqueTopics = uniqueTopics
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var article = await db.Articles.FindAsync(id);
            if(article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        [HttpPost]
        public IActionResult OnSearch(string tag, int page = 1, int pageSize = 10)
        {
            var totalArticles = db.Articles.Count();
            var totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            
            var articles = db.Articles
                .Where(a => a.Topic.ToLower() == tag.ToLower()) 
                .OrderByDescending(a => a.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var uniqueTopics = articles.Select(t => t.Topic).Distinct().ToList();

            // Create view model
            var model = new ArticleViewModel
            {
                Articles = articles,
                CurrentPage = page,
                TotalPages = totalPages,
                ItemsPerPage = pageSize,
                UniqueTopics = uniqueTopics
            };

            return View("Index", model);
        }
    }
}