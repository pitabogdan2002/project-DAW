using Microsoft.AspNetCore.Identity;

namespace crowd_knowledge_contribution.Models
{
    public class ApplicationUser : IdentityUser
    {

        public List<Article> Favorites { get; set; } = new List<Article>();
    }
}
