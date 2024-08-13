namespace InnoShop.Services.ProductAPI.Models.DTO
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public bool IsAvailable { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
    }
}
