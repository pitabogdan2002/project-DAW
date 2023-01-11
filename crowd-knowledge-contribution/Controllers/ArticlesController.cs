using crowd_knowledge_contribution.Data;
using crowd_knowledge_contribution.Data.Migrations;
using crowd_knowledge_contribution.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Edit = crowd_knowledge_contribution.Models.Edit;
using Favorite = crowd_knowledge_contribution.Models.Favorite;

namespace crowd_knowledge_contribution.Controllers
{
    [Authorize]
    public class ArticlesController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ArticlesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Index(string Criteriul)
        {
            var articles = db.Articles.Include("Category").Include("User");
            switch (Criteriul)
            {
                case "Alfabetic":
                    articles = articles.OrderBy(s => s.Title);
                    break;
                case "Alfabetic_Inv":
                    articles = articles.OrderByDescending(s => s.Title);
                    break;
                case "Data":
                    articles = articles.OrderBy(s => s.Date);
                    break;
                case "Data_Inv":
                    articles = articles.OrderByDescending(s => s.Date);
                    break;
                default:
                    articles = articles.OrderByDescending(s => s.Date);
                    break;
            }

            var search = "";

            // MOTOR DE CAUTARE

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

                // Cautare in articol (Title si Content)

                List<int> articleIds = db.Articles.Where
                                        (
                                         at => at.Title.Contains(search)
                                         || at.Content.Contains(search)
                                        ).Select(a => a.Id).ToList();

                // Cautare in comentarii (Content)
                List<int> articleIdsOfCommentsWithSearchString = db.Comments
                                        .Where
                                        (
                                         c => c.Content.Contains(search)
                                        ).Select(c => (int)c.ArticleId).ToList();

                // Se formeaza o singura lista formata din toate id-urile selectate anterior
                List<int> mergedIds = articleIds.Union(articleIdsOfCommentsWithSearchString).ToList();


                // Lista articolelor care contin cuvantul cautat
                // fie in articol -> Title si Content
                // fie in comentarii -> Content
                articles = db.Articles.Where(article => mergedIds.Contains(article.Id))
                                      .Include("Category")
                                      .Include("User")
                                      .OrderBy(a => a.Date);

            }

            ViewBag.SearchString = search;

            // AFISARE PAGINATA

            // Alegem sa afisam 3 articole pe pagina
            int _perPage = 3;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }


            // Fiind un numar variabil de articole, verificam de fiecare data utilizand 
            // metoda Count()

            int totalItems = articles.Count();


            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Articles/Index?page=valoare

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3 
            // Asadar offsetul este egal cu numarul de articole care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau articolele corespunzatoare pentru fiecare pagina la care ne aflam 
            // in functie de offset
            var paginatedArticles = articles.Skip(offset).Take(_perPage);


            // Preluam numarul ultimei pagini

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem articolele cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Articles = paginatedArticles;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?page";
            }


            Article article = new Article();

            article.Categ = GetAllCategories();

            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;

            ViewBag.Categorii = categories;
            // ViewBag.OriceDenumireSugestiva
 
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View(article);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Protect(int id)
        {
            
          Article article = db.Articles.Include("Category")
                                       .Where(art => art.Id == id)
                                       .First();

            if (User.IsInRole("Admin"))
                if(article.Protected=="Unprotected")
            {
                db.Articles.Find(id).Protected = "Protected";
                db.SaveChanges();
            }
            else
                {
                    db.Articles.Find(id).Protected = "Unprotected";
                    db.SaveChanges();
                }
            return Redirect("/Articles/Show/" + article.Id);
        }


        public IActionResult AddFavorite(int id)
        {

            var result = db.Favorites.Where(art => art.ArticleId == id && art.UserId == _userManager.GetUserId(User));
            if (result.Count() == 0)
            {
                Favorite fav = new Favorite();

                fav.ArticleId = id;
                fav.UserId = _userManager.GetUserId(User);
                db.Favorites.Add(fav);
                db.SaveChanges();
            }
            else
            {
                Favorite fav = db.Favorites.Where(art => art.ArticleId == id && art.UserId == _userManager.GetUserId(User)).First();
                db.Favorites.Remove(fav);
                db.SaveChanges();
            }
            return Redirect("/Articles/Show/" + id);
        }


        public IActionResult UndoEdit(int id)
        {

            Article article = db.Articles.Include("Category")
                                         .Where(art => art.Id == id)
                                         .First();

            var result = db.Edits.Where(ed => ed.ArticleId == article.Id);
            if(result.Count()!=0)
            { Edit editare = db.Edits.Where(ed => ed.ArticleId == article.Id).OrderByDescending(ed =>ed.tId).First();
            
                db.Articles.Find(id).Content = editare.Content;
                db.Articles.Find(id).Title = editare.Title;
                db.Edits.Remove(editare);

                db.SaveChanges();
            }
            
            return Redirect("/Articles/Show/" + article.Id);
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Show(int id)
        {
            Article article = db.Articles.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == id)
                                         .First();

            SetAccessRights();
            ViewBag.Proteced = article.Protected;
            var result = db.Favorites.Where(art => art.ArticleId == id && art.UserId == _userManager.GetUserId(User));
            if (result.Count() == 0)
            {
                ViewBag.Favorit = "nu";
            }
            else
            {
                ViewBag.Favorit = "da";
            }

            return View(article);

        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;
            if (User.IsInRole("Editor"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        [HttpPost]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comment.ArticleId);
            }

            else
            {
                Article art = db.Articles.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == comment.ArticleId)
                                         .First();
                
                SetAccessRights();

                //return Redirect("/Articles/Show/" + comm.ArticleId);

                return View(art);
            }
        }




        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New()
        {

            Article article = new Article();

            article.Categ = GetAllCategories();

            return View(article);
        }

        // Se adauga articolul in baza de date
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]

        public IActionResult New(Article article)
        {
            var sanitizer = new HtmlSanitizer();
            article.Date = DateTime.Now;
            article.Categ = GetAllCategories();
            article.Content = sanitizer.Sanitize(article.Content);
            article.UserId = _userManager.GetUserId(User);

            //if (ModelState.IsValid)
            //{
            //    db.Articles.Add(article);
            //    db.SaveChanges();
            //    TempData["message"] = "Articolul a fost adaugat";
            //    return RedirectToAction("Index");

            //}
            //else
            //{
            //    return View(article);
            //}
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
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {

            Article article = db.Articles.Include("Category")
                                        .Where(art => art.Id == id)
                                        .First();

            article.Categ = GetAllCategories();


            if (article.UserId == _userManager.GetUserId(User) && article.Protected == "Unprotected" || User.IsInRole("Admin"))
            {
                return View(article);
            }
            else
            {
                TempData["message"] = "Nu puteti edita acest articol deoarece nu va apartine";
                return RedirectToAction("Index");
            }

        }

        // Se adauga articolul modificat in baza de date
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id, Article requestArticle)
        {
            var sanitizer = new HtmlSanitizer();

            Article article = db.Articles.Find(id);

            Edit edit = new Edit();

            edit.Title = article.Title;
            edit.Content = article.Content;
            edit.ArticleId = article.Id;
            db.Edits.Add(edit);
            db.SaveChanges();

            requestArticle.Categ = GetAllCategories();

            try
            {
                if (article.UserId == _userManager.GetUserId(User) && article.Protected == "Unprotected" || User.IsInRole("Admin"))
                {
                    article.Title = requestArticle.Title;
                    requestArticle.Content = sanitizer.Sanitize(requestArticle.Content);
                    article.Content = requestArticle.Content;
                    article.Date = requestArticle.Date;
                    article.CategoryId = requestArticle.CategoryId;
                    db.SaveChanges();
                    TempData["message"] = "Articolul a fost modificat";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu puteti edita acest articol deoarece nu va apartine";
                    return RedirectToAction("Index");
                }

            }
            catch (Exception e)
            {
                requestArticle.Categ = GetAllCategories();
                return View(requestArticle);
            }
        }


        // Se sterge un articol din baza de date 
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Delete(int id)
        {
            Article article = db.Articles.Include("Comments")
                                         .Where( art=> art.Id == id)
                                         .First();

            if (article.UserId == _userManager.GetUserId(User) && article.Protected == "Unprotected" || User.IsInRole("Admin"))
            {
                db.Articles.Remove(article);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost sters";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti acest articol";
                return RedirectToAction("Index");   
            }
                
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
