namespace KanbanModel.DTOs.ResponseDTOs
{
    public class LinkDTO
    {
        public string Rel { get; set; } = string.Empty; // e.g., "self", "update", "delete"
        public string Href { get; set; } = string.Empty; // URL
        public string Method { get; set; } = string.Empty; // HTTP method

        public LinkDTO(string rel, string href, string method)
        {
            Rel = rel;
            Href = href;
            Method = method;
        }
    }
}
