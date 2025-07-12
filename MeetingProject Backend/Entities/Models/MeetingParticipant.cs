using Entities.IModels;

namespace Entities.Models
{
    public class MeetingParticipant : IEntity
    {
        public int Id { get; set; }  // Katılım ID'si

        // Foreign key: Katılımcı olan kullanıcının ID'si
        public int? UserId { get; set; }
        public virtual User User { get; set; }  // Katılımcı olan kullanıcı bilgileri

        // Foreign key: Katılımcının katıldığı toplantı ID'si
        public int MeetingId { get; set; }
        public virtual Meeting Meeting { get; set; }  // Katılımcının katıldığı toplantı
    }
}
