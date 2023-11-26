using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using test.Models;

namespace test.Controllers
{
    [Authorize(Roles = "User, Administrator")]
	public class ProfileController : Controller
    {
        private readonly AppDbContext db;

		public ProfileController(AppDbContext db)
		{
			this.db = db;
		}


        public async Task<ActionResult> Index()
        {
            var nicknameClaim = HttpContext.User.FindFirst(ClaimTypes.Name);

            if(nicknameClaim == null) 
            {
                return NotFound();
            }

            var account = await db.Accounts.Where(user => user.Nickname == nicknameClaim.Value).SingleOrDefaultAsync();
            if(account == null)
            {
                return NotFound();
            }

            var roleColumn = db.Roles.FirstOrDefault(r => r.Id == account.Role);

            var model = new AccountViewModel
            {
                Email = account.Email,
                Hash = account.Hash,
                Salt = account.Salt,
                Nickname = account.Nickname,
                RoleName = roleColumn.Role
            };

            return View(model);
        }
    }
}