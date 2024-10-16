using IdentityTest.Models;
using IdentityTest.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace IdentityTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpPost("register")]

        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,

                };
               var result = await _userManager.CreateAsync(user,model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Ok(new
                    {
                        message = "Kayıt Başarılı"
                    });
                }
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });


            }
            return BadRequest(new
            {
                errors= ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage)
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return Ok(new { message = "Giriş Başarılı" }); 
                }
                else
                {
                    return Unauthorized(new { message = "Kullanıcı Adı Veya Şifre Hatalı" });
                }

            }

            return BadRequest(new
            {
                errors = ModelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage)
            });

        }


        [HttpPost("createrole")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
              var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                
                if (result.Succeeded)
                {
                    return Ok(new { message = "Giriş Başarılı.." });
                }
                else
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }
            }

            return BadRequest(new { message = "Rol Adı Boş Olamaz." });
        }

        [HttpGet("GetRole")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        [HttpPost("AddToRole")]
        public async Task<IActionResult> AddToRole(AddToRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null)
            {
                return NotFound(new { message = "Kullanıcı bulunamadı.." });
            }

            if(!await _roleManager.RoleExistsAsync(model.RoleName))
             {
                return NotFound(new { message = "Role Bulunamadı.." });
            }

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (result.Succeeded)
            {
                return Ok(new { message = "Rol Eklendi" });
            }
            else
            {
                return BadRequest(new { errors = result.Errors.Select(x => x.Description) });
            }

        }

        [HttpGet("userroles/{userId}")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if(user is null)
            {
                return NotFound(new { message = "Kullanıcı bulunamadı.." });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(roles);
        }

    }
}
