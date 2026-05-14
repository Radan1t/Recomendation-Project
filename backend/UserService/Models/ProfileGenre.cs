namespace UserService.Entities
{
    public class ProfileGenre
    {
        public int UserID { get; set; }
        public UserProfile UserProfile { get; set; }
        public int GenreID { get; set; }
        public Genre Genre { get; set; }
    }
}
