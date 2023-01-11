using crowd_knowledge_contribution.Data;
using crowd_knowledge_contribution.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace crowd_knowledge_contribution.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public FavoritesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var favorite = db.Favorites.Where(fav => fav.UserId == _userManager.GetUserId(User)).Include("Article");
            if (favorite.Count() > 0)
            {
                ViewBag.Mesaj = "da";
                ViewBag.favorite = favorite;
            }
            else
            {
                ViewBag.Mesaj = "nu";
            }

            return View();
        }
    }
}
