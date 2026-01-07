using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // ID задаємо вручну (він приходить з AuthService)
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime DateOfBirth { get; set; }
        
        // Зв'язок 1 до 1
        public UserProfile UserProfile { get; set; }
    }
}