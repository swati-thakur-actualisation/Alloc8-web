using System.ComponentModel.DataAnnotations;

namespace Alloc8_web.Model
{
    public class Alloc8ChatGptHistory
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserPrompt { get; set; }
        public string BotResponse { get; set; }
        public int ChatSessionId { get; set; }

        public bool isDeleted { get; set; }
        public Alloc8ChatSession ChatSession { get; set; }
        public DateTime Timestamp { get; set; }

        
    }
    public class Alloc8ChatSession
    {
        [Key]
        public int Id { get; set; }
        public List<Alloc8ChatGptHistory> Messages { get; set; } = new List<Alloc8ChatGptHistory>();
    }
}
