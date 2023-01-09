using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crowd_knowledge_contribution.Models
{
    public class Edit
    {
        [Key]
        public int tId { get; set; }

        public int ArticleId { get; set; }

        public string Title { get; set; }
        [Required(ErrorMessage = "Continutul articolului este obligatoriu")]
        public string Content { get; set; }
        public virtual Article Article { get; set; }
    }
}
