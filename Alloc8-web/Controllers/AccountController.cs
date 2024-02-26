// AccountController.cs
using Alloc8_web.Controllers;
using Alloc8_web.Validations;
using Alloc8_web.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Alloc8.ef;
using Alloc8.ef.Entities.Dashboard;
public class AccountController : Controller
{
    private readonly SignInManager<Alloc8.ef.Entities.Dashboard.ApplicationUser> _signInManager;
    private UserManager<Alloc8.ef.Entities.Dashboard.ApplicationUser> _userManager;
    private Alloc8DbContext _context;

    public AccountController(SignInManager<Alloc8.ef.Entities.Dashboard.ApplicationUser> signInManager, UserManager<Alloc8.ef.Entities.Dashboard.ApplicationUser> userManager, Alloc8DbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return Redirect("/Home/Index");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return ModelStateValidator.throwValidationErrors(this);
        }

        var user = await _userManager.FindByEmailAsync(model.email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid Email");
            return View(model);
        }
        if (!user.isActive)
        {
            ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact the Actualisation group.");
            return View(model);
        }
        var result = await _signInManager.PasswordSignInAsync(model.email, model.password, model.rememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid Password");
            return View(model);
        }

        user.loged_at = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return RedirectToLocal(returnUrl);
    }


    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
        }
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/Account/Login");
    }
    [HttpPost]
    public async Task<IActionResult> LoginAs(string userId)
    {
        try
        {
            if (userId == null)
            {
                return NotFound();
            }
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                await _signInManager.RefreshSignInAsync(user);

            }
        }
        catch (Exception ex)
        {
            return NotFound();
        }
        return Ok(new { status = 1 });

    }
    [HttpGet]
    public IActionResult cantSignIn(string email)
    {
        return View(new ForgotPasswordViewModel { email = email});
    }
    public async Task<IActionResult> sendForgotPasswordEmail(string email)
    {
        var user = await _context.users.Where(x => x.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            return BadRequest(new {error = "Invalid Email"});
        }
        try
        {

            
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string? callbackUrl = Url.Action("changePassword", "Account", new { email = user.Email, token }, protocol: HttpContext.Request.Scheme);
            if(callbackUrl == null) {
                return BadRequest(new {error = "Unable to send reset passsword link"});
            }
           
            

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        return Ok();
    }
    public async Task<IActionResult> changePassword(string email,string token)
    {
        Alloc8.ef.Entities.Dashboard.ApplicationUser? user = await _context.users.FirstOrDefaultAsync(x=>x.Email == email);
        if(user == null)
        {
            return NotFound();
        }
        return View(new UserChangePasswordViewModal { email = email,token=token});
    }
    [HttpPost]
    public async Task<IActionResult> changePassword(UserChangePasswordViewModal modal)
    {
        if (!ModelState.IsValid)
        {
            return ModelStateValidator.throwValidationErrors(this);
        }

        Alloc8.ef.Entities.Dashboard.ApplicationUser? user = await _context.users.FirstOrDefaultAsync(x => x.Email == modal.email);
        if (user == null)
        {
            return BadRequest(new { message = "Unable to Resest your password" });
        }
        var result = await _userManager.ResetPasswordAsync(user, modal.token, modal.password);
        if(!result.Succeeded) 
        {
            return BadRequest(new { message = "Unable to Resest your password"});
        }
        return Ok(new {status = 1});
    }
}
