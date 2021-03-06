using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FinanceTracker.Models;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FinanceTracker.Services;

namespace FinanceTracker.Controllers
{
    [EnableCors("CORSPolicy")]
    // [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;

        public TokenController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, TokenService token)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _tokenService = token;
        }

        [HttpGet]
        [Route("api/token/test")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult test()
        {
            var email = User.FindFirst(ClaimTypes.Email).Value;
            var user = _userManager.FindByEmailAsync(email);
            Console.WriteLine(ClaimTypes.Email.ToString());
            return Ok(new {user.Result});
        }

        [HttpPost]
        [Route("api/register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser{ 
                    UserName= model.UserName, 
                    Email = model.Email,    
                    FirstName = model.FirstName, 
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // await _signInManager.SignInAsync(user, isPersistent: false);
                    var response = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
                    if (!response.Succeeded)
                    {
                        return Unauthorized();
                    }
                    
                    var UserName = user.UserName;
                    return Ok(new { token = _tokenService.GetToken(user), UserName });
                }
                else
                {
                    return Unauthorized();
                }
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("api/login")]
        public async Task<IActionResult> Signin([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

                    if (!result.Succeeded)
                    {
                        return Unauthorized();
                    }
                   
                    var UserName = user.UserName;
                    return Ok(new { token = _tokenService.GetToken(user), UserName });
                }
                else{
                    return NotFound();
                }
            }

            return BadRequest();
        }

    }
}