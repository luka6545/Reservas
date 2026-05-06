using Microsoft.AspNetCore.Mvc;
using ReservaWeb.Models;
using ReservaWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ReservaWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var token = await _apiService.LoginAsync(model.Email, model.Password);

            if (token != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // EXTRAER DATOS MANUALMENTE DEL TOKEN
                // Buscamos el nombre y el rol sin importar si el nombre es largo o corto
                var nombre = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == ClaimTypes.Name)?.Value ?? model.Email;
                var rol = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value ?? "Cliente";

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, nombre),
            new Claim(ClaimTypes.Role, rol) 
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                HttpContext.Response.Cookies.Append("TokenJWT", token, new CookieOptions { HttpOnly = true });

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Credenciales incorrectas.";
            return View(model);
        }
        // --- VISTA REGISTRO ---
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegistroViewModel model)
        {
            if (model.Password != model.ConfirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View(model);
            }

            var exito = await _apiService.RegistrarAsync(model.Nombre, model.Email, model.Password);
            if (exito) return RedirectToAction("Login");

            ViewBag.Error = "No se pudo registrar. El correo ya podría estar en uso.";
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Response.Cookies.Delete("TokenJWT");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}