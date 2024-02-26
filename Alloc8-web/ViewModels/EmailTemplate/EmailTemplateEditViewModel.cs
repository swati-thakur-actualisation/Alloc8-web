namespace Alloc8_web.ViewModels.EmailTemplate
{
    public class EmailTemplateViewModel
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? title { get; set; }
        public string? notes { get; set; }
        public string? lastUpdatedBy { get; set; }
        public DateTime? lastIpdatedAt { get; set; }
    }
}
