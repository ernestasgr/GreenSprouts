using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test.Models;
using System.Security.Cryptography;
using System;
using System.Text;
using test.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


namespace test.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext db;

		public AccountController(AppDbContext db)
		{
			this.db = db;
		}

        public async Task<IActionResult> Index(int delete = 0, int edit = 0)
		{
			if(delete != 0)
			{
				var account = await db.Accounts.FindAsync(delete);
				if(account != null)
				{
					db.Accounts.Remove(account);
					await db.SaveChangesAsync();
				}
			}
			else if (edit != 0)
			{
				var user = await db.Accounts.FindAsync(edit);
				if (user == null)
					return NotFound();
				return View(user);
			}
			return View(await db.Accounts.ToListAsync());
		}


        [HttpPost]
		public async Task<IActionResult> Index([FromForm] Account account)
		{
			if (!string.IsNullOrEmpty(account.Nickname))
			{
				var edit = await db.Accounts.FindAsync(account.Nickname);
				if (edit == null)
                {
                    return NotFound();
                }
					
                edit.Nickname = account.Nickname;
                edit.Email = account.Email;
                edit.Hash = account.Hash;
				edit.Salt = account.Salt;
                edit.Role = account.Role;
				await db.SaveChangesAsync();
			}
			else
			{
				await db.Accounts.AddAsync(account);
				await db.SaveChangesAsync();
			}
			return LocalRedirect("/");
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[HttpPost("/register")]
		public async Task<IActionResult> Register(string nickname, string email, string password)
		{
    		List<string> errors = new List<string>();

    		if (string.IsNullOrWhiteSpace(nickname))
    		{
        		errors.Add("Please provide a nickname.");
    		}

    		if (string.IsNullOrWhiteSpace(email))
    		{
        		errors.Add("Please provide an email address.");
    		}

    		if (string.IsNullOrWhiteSpace(password))
    		{
        		errors.Add("Please provide a password.");
    		}

    		var user = await db.Accounts.FirstOrDefaultAsync(u => u.Email == email);
    		if (user != null)
    		{
        		errors.Add("A user with that email address already exists.");
    		}

    		var user1 = await db.Accounts.FirstOrDefaultAsync(u => u.Nickname == nickname);
    		if (user1 != null)
    		{
        		errors.Add("A user with that nickname already exists.");
    		}

    		if (errors.Any())
    		{
        		return BadRequest(errors);
    		}

			var encrypter = new Encrypter();
    		byte[] salt = encrypter.GenerateSalt();
    		var hashedPassword = encrypter.HashPassword(password, salt);

    		Random random = new Random();
    		var newUser = new Account
    		{
        		Nickname = nickname,
        		Email = email,
        		Hash = hashedPassword,
        		Salt = Convert.ToBase64String(salt),
        		Role = 1
    		};

    		db.Accounts.Add(newUser);
    		await db.SaveChangesAsync();

    		return Ok();
		}

        [HttpPost("/login")]
        public async Task<IActionResult> Login(string NickName, string Password)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(NickName))
            {
                errors.Add("Please provide a nickname.");
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                errors.Add("Please provide a password.");
            }

            if (errors.Any())
            {
                return BadRequest(errors);
            }

            var user = await db.Accounts.FirstOrDefaultAsync(u => u.Nickname == NickName);
            if (user == null)
            {
                errors.Add("Invalid nickname or password.");
                return BadRequest(errors);
            }

            try
            {
                byte[] data = Convert.FromBase64String(user.Salt);

                var salt = Convert.FromBase64String(user.Salt);
                var encrypter = new Encrypter();
                var hashedPassword = encrypter.HashPassword(Password, salt);

                if (!encrypter.SlowEquals(Convert.FromBase64String(user.Hash), Convert.FromBase64String(hashedPassword)))
                {
                    errors.Add("Invalid nickname or password.");
                    return BadRequest(errors);
                }


                HttpContext.Session.SetString("Nickname", user.Nickname);

                // Set the Nickname value in the User.Identity object
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Nickname));

				string roleName = GetRoleName(user.Role);
				identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return Json(new { success = true, nickname = user.Nickname });
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return BadRequest(errors);
            }

        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

		public string GetRoleName(int? role_level) 
		{
			switch(role_level)
			{
				case 1:
					return "User";
				case 2:
					return "Administrator";
				default:
					return "none";
			}
		}

		[HttpPost("/forgotPassword")]
		public async Task<IActionResult> ForgotPassword(string email)
		{
    		List<string> errors = new List<string>();
		
    		if (string.IsNullOrWhiteSpace(email))
    		{
        		errors.Add("Please provide an email address.");
    		}

    		var user = await db.Accounts.FirstOrDefaultAsync(u => u.Email == email);

    		if (errors.Any())
    		{
        		return BadRequest(errors);
    		}

    		if(user!= null)
			{
				int passwordLength = 15; 
        		string password = GenerateRandomPassword(passwordLength);
				var encrypter = new Encrypter();
				byte[] salt = encrypter.GenerateSalt();
    			var hashedPassword = encrypter.HashPassword(password, salt);
				user.Hash = hashedPassword;
				user.Salt = Convert.ToBase64String(salt);
    			await db.SaveChangesAsync();

				// send email with the new password
				string message = $"Your temporary new password is: {password}";
				string subject = "Password Reset";
				await SendEmailAsync(email, message, subject);
				
			}

    		return Ok();
		}

		static string GenerateRandomPassword(int length) 
		{
        	const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        	byte[] randomBytes = new byte[length];
        	using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) 
			{
            	rng.GetBytes(randomBytes);
        	}
        	StringBuilder sb = new StringBuilder();
        	foreach (byte b in randomBytes) 
			{
            	sb.Append(validChars[b % validChars.Length]);
        	}
        	return sb.ToString();
    	}

		
		public async Task SendEmailAsync(string email, string message, string subject)
		{
    		var messageToSend = new MimeMessage();
    		messageToSend.From.Add(new MailboxAddress("Your Name", "testasgreenprogramming@outlook.com"));
    		messageToSend.To.Add(new MailboxAddress("", email));
    		messageToSend.Subject = subject;

    		messageToSend.Body = new TextPart("plain")
    		{
        		Text = message
    		};

    		using (var client = new SmtpClient())
    		{
        		await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
        		await client.AuthenticateAsync("testasgreenprogramming@outlook.com", "testas222"); // kol kas cia info poto perkelsim kur saugu :))
        		await client.SendAsync(messageToSend);
        		await client.DisconnectAsync(true);
    		}
		}

		[HttpPost("/checkEmailExists")]
        public async Task<IActionResult> CheckEmailExists(string email)
        {    

            var user = await db.Accounts.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
               return Ok();
            }
			else
			{
				return BadRequest("Email does exist already");
			}	        

        }

		[HttpPost("/googleLogin")]
        public async Task<IActionResult> GoogleLogin(string email)
        {    

			List<string> errors = new List<string>();

            var user = await db.Accounts.FirstOrDefaultAsync(u => u.Email == email);
            try
            {

                HttpContext.Session.SetString("Nickname", user.Nickname);

                // Set the Nickname value in the User.Identity object
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Nickname));

				string roleName = GetRoleName(user.Role);
				identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return Json(new { success = true, nickname = user.Nickname });
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return BadRequest(errors);
            }

        }

		[HttpPost("/googleRegister")]
        public async Task<IActionResult> googleRegister(string email, string nickname)
        {    			
			List<string> errors = new List<string>();
			string NicknameTrimmed = nickname.Substring(9);
            var user = await db.Accounts.FirstOrDefaultAsync(u => u.Email == email);
			var user1 = await db.Accounts.FirstOrDefaultAsync(u => u.Nickname == NicknameTrimmed);
			if (user1 != null)
    		{
        		errors.Add("A user with that nickname already exists.");
    		}
			if (errors.Any())
    		{
        		return BadRequest(errors);
    		}
			
            int passwordLength = 15; 
			string password = GenerateRandomPassword(passwordLength);
			var encrypter = new Encrypter();
			byte[] salt = encrypter.GenerateSalt();
    		var hashedPassword = encrypter.HashPassword(password, salt);
            var newUser = new Account
    		{
        		Nickname = NicknameTrimmed,
        		Email = email,
				Hash = hashedPassword,
				Salt = Convert.ToBase64String(salt),
        		Role = 1
    		};

    		db.Accounts.Add(newUser);
    		await db.SaveChangesAsync();
            
            try
            {
				var user3 = await db.Accounts.FirstOrDefaultAsync(u => u.Nickname == NicknameTrimmed);
                HttpContext.Session.SetString("Nickname", user3.Nickname);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, user3.Nickname));

				string roleName = GetRoleName(user3.Role);
				identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return Json(new { success = true, nickname = user3.Nickname });
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return BadRequest(errors);
            }

        }
    }
}