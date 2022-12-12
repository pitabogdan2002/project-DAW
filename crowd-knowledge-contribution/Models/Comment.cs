using System.ComponentModel.DataAnnotations;

namespace crowd_knowledge_contribution.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int ArticleId { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Article Article { get; set; }
    }
}
