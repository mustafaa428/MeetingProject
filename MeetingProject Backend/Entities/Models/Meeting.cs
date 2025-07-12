using Entities.IModels;

namespace Entities.Models
{
    public class Meeting : IEntity
    {
        public int Id { get; set; }  // Toplantı ID'si
        public string MeetId { get; set; }  // Benzersiz toplantı ID'si
        public string Title { get; set; }  // Toplantı başlığı
        public int UserId { get; set; }  // Toplantıyı oluşturan kullanıcının ID'si
        public DateTime CreatedAt { get; set; }  // Toplantının oluşturulma tarihi

        public virtual User User { get; set; }  // Kullanıcı ile ilişkisi

        // Toplantının katılımcıları
        public virtual ICollection<MeetingParticipant> MeetingParticipants { get; set; }
    }
}
