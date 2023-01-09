namespace crowd_knowledge_contribution.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Article Article { get; set; }

    }
}
