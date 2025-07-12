using Entities.IModels;
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class User : IEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MinLength(6)]
        public string Password { get; set; }

        // Kullanıcı oluşturulma tarihi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }  // Silinme durumu (Soft delete için)

        // Kullanıcının oluşturduğu toplantılar (Toplantı oluşturulduğunda ilişki eklenir)
        public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();

        // Kullanıcının katıldığı toplantılar (Katılımcı olarak sonradan eklenebilir)
        public ICollection<MeetingParticipant> MeetingParticipants { get; set; } = new List<MeetingParticipant>();
    }
}
