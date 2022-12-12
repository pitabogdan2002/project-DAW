using crowd_knowledge_contribution.Data;
using crowd_knowledge_contribution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace crowd_knowledge_contribution.Controllers
{
    public class ArticlesController : Controller
    {

        private readonly ApplicationDbContext db;
        public ArticlesController(ApplicationDbContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            var articles = db.Articles.Include("Category");

            Article article = new Article();

            article.Categ = GetAllCategories();

            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;

            ViewBag.Categorii = categories;
            // ViewBag.OriceDenumireSugestiva
            ViewBag.Articles = articles;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View(article);
        }

        public IActionResult Show(int id)
        {
            Article article = db.Articles.Include("Category").Include("Comments")
                               .Where(art => art.Id == id)
                               .First();


            return View(article);
        }

        public IActionResult New()
        {

            Article article = new Article();

            article.Categ = GetAllCategories();

            return View(article);
        }

        // Se adauga articolul in baza de date
        [HttpPost]
        public IActionResult New(Article article)
        {
            article.Date = DateTime.Now;
            article.Categ = GetAllCategories();

            try
            {
                db.Articles.Add(article);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost adaugat";
                return RedirectToAction("Index");
            }

            catch (Exception)
            {
                return View(article);
            }
        }

        // Se editeaza un articol existent in baza de date impreuna cu categoria din care face parte
        // Categoria se selecteaza dintr-un dropdown
        // HttpGet implicit
        // Se afiseaza formularul impreuna cu datele aferente articolului din baza de date
        public IActionResult Edit(int id)
        {

            Article article = db.Articles.Include("Category")
                                        .Where(art => art.Id == id)
                                        .First();

            article.Categ = GetAllCategories();

            return View(article);

        }

        // Se adauga articolul modificat in baza de date
        [HttpPost]
        public IActionResult Edit(int id, Article requestArticle)
        {
            Article article = db.Articles.Find(id);
            requestArticle.Categ = GetAllCategories();

            try
            {
                article.Title = requestArticle.Title;
                article.Content = requestArticle.Content;
                article.Date = requestArticle.Date;
                article.CategoryId = requestArticle.CategoryId;
                db.SaveChanges();
                TempData["message"] = "Articolul a fost modificat";
                return RedirectToAction("Index");

            }
            catch (Exception e)
            {
                return View(requestArticle);
            }
        }


        // Se sterge un articol din baza de date 
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Article article = db.Articles.Find(id);
            db.Articles.Remove(article);
            db.SaveChanges();
            TempData["message"] = "Articolul a fost sters";
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName.ToString();

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }
    }
}
