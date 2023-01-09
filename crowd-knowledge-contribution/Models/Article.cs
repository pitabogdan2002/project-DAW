using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crowd_knowledge_contribution.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Continutul articolului este obligatoriu")]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Categoria este obligatorie")]

        public string Protected { get; set; } = "Unprotected";

        public int CategoryId { get; set; }

        public string? UserId { get; set; }



        public virtual ApplicationUser? User { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Edit> Edits { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Categ { get; set; }
    }
}
