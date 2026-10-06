using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.Models
{
    public class Category
    {
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public required string Name { get; set; }

        public Guid GroupId { get; set; }
        public Group? Group { get; set; }

        public ICollection<ShoppingItem> ShoppingItems { get; set; }
            = new List<ShoppingItem>();
    }
}