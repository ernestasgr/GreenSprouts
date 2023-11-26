using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using test.Utility;
using test.Models;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace test.Controllers
{
    [Authorize(Roles = "Administrator")]
	public class AdminController : Controller
    {

        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [Route("Admin/Users")]
        [HttpGet]
        public async Task<IActionResult> Users(string nickname)
        {
            var users = _context.Accounts.ToList();
            
            if (!string.IsNullOrEmpty(nickname))
            {
                users = users.Where(u => u.Nickname.Contains(nickname)).ToList();
            }

            var accountViewModels = users
                .Select(u => new AccountViewModel
                {
                    Nickname = u.Nickname,
                    Hash = u.Hash,
                    Salt = u.Salt,
                    Email = u.Email,
                    RoleName = _context.Roles.FirstOrDefault(r => r.Id == u.Role)?.Role
                })
                .ToList();

            return View("Users/Index", accountViewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateUser() 
        {
            return View("Users/CreateUser");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CreateUser")]
        public IActionResult CreateUser(AccountViewModel user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var encrypter = new Encrypter();
                byte[] salt = encrypter.GenerateSalt();
                var hashedPassword = encrypter.HashPassword(user.Hash, salt);

                _context.Accounts.Add(new Account{
                    Email = user.Email,
                    Hash = hashedPassword,
                    Role = user.Role,
                    Nickname = user.Nickname,
                    Salt = Convert.ToBase64String(salt)
                });
                _context.SaveChanges();

                return RedirectToAction("Users", "Admin");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating user: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateUser(string nickname) 
        {
            var account = await _context.Accounts.FindAsync(nickname);

            var accountViewModel = new AccountViewModel 
            {
                Email = account.Email,
                Hash = account.Hash,
                Nickname = account.Nickname,
                Role = account.Role,
                RoleName = _context.Roles.FirstOrDefault(r => r.Id == account.Role).Role,
                Salt = account.Salt
            };

            return View("Users/UpdateUser", accountViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateUser(AccountViewModel updatedAccount)
        {
            var existingAccount = await _context.Accounts.FindAsync(updatedAccount.Nickname);

            if (existingAccount != null)
            {
                existingAccount.Nickname = updatedAccount.Nickname;
                existingAccount.Email = updatedAccount.Email;
                existingAccount.Role = updatedAccount.Role;

                var password_updated = existingAccount.Hash != updatedAccount.Hash;
                var encrypter = new Encrypter();
                if(password_updated) 
                {
                    byte[] salt = encrypter.GenerateSalt();
                    var hashedPassword = encrypter.HashPassword(updatedAccount.Hash, salt);

                    existingAccount.Hash = hashedPassword;
                    existingAccount.Salt = Convert.ToBase64String(salt);
                }
                else 
                {
                    existingAccount.Hash = updatedAccount.Hash;
                    existingAccount.Salt = updatedAccount.Salt;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Users", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string nickname)
        {
            var account = await _context.Accounts.FindAsync(nickname);
            if (account == null)
            {
                return NotFound();
            }
            return View("Users/DeleteUser", account);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string nickname)
        {
            var account = await _context.Accounts.FindAsync(nickname);
            if (account == null)
            {
                return NotFound();
            }
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users", "Admin");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ProgrammingTasks()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ShutDownVM()
        {
            throw new Exception("All good :)");
        }
    }
}