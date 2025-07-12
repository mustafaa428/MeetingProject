using DataBusiness.Interface;
using Entities.Models;
using Services.Interface;

namespace Services.Services
{
    public class MeetingService(IRepository<Meeting> meetingRepository, IRepository<MeetingParticipant> meetingParticipantRepository) : IMeetingService
    {
        private readonly Random _random = new Random();

        public Meeting CreateMeeting(int userId, string title, List<int>? participantIds)
        {
            // Benzersiz MeetId oluşturma kısmı
            var meetId = GenerateMeetId();

            // GetAll() kullanarak benzersiz MeetId kontrolü
            var existingMeetIds = meetingRepository.GetAll().Select(m => m.MeetId).ToHashSet();

            while (existingMeetIds.Contains(meetId))
            {
                meetId = GenerateMeetId();
            }


            // Toplantıyı oluştur
            var meeting = new Meeting
            {
                UserId = userId,  // Toplantıyı oluşturan kullanıcı
                Title = title,
                MeetId = meetId,  // Toplantının MeetId'si
                CreatedAt = DateTime.UtcNow
            };

            // Toplantıyı veritabanına kaydet
            meetingRepository.Add(meeting);

            // Eğer katılımcılar belirtilmişse, onları ilişkilendir
            if (participantIds != null && participantIds.Any())
            {
                foreach (var participantId in participantIds)
                {
                    var meetingParticipant = new MeetingParticipant
                    {
                        UserId = participantId,
                        MeetingId = meeting.Id
                    };

                    meetingParticipantRepository.Add(meetingParticipant);
                }
            }

            // Sadece oluşturulan toplantıyı döndür (link olmadan)
            return meeting;
        }

        public bool AddParticipantToMeeting(string meetingId, int userId)
        {
            // Toplantının var olup olmadığını kontrol et
            var meeting = meetingRepository.GetAll()
                .FirstOrDefault(m => m.MeetId == meetingId & m.UserId != userId);
            var meeting2 = meetingRepository.GetAll().Where(m => m.MeetId == meetingId & m.UserId != userId).Select(m => m.MeetId);

            if (meeting == null)
            {
                // Toplantı bulunamadığında false döndür
                return false;
            }


            // Kullanıcı zaten katılımcı mı kontrol et
            var isAlreadyParticipant = meetingParticipantRepository
                .GetAll()
                .Any(mp => mp.MeetingId == meeting.Id && mp.UserId == userId);
            // yukarıdaki sorgu ile joinleyip tek bir sorgu yap. en az sorgu yapmak önemli olan.

            if (!isAlreadyParticipant)
            {
                // Yeni katılımcıyı ekle
                var newParticipant = new MeetingParticipant
                {
                    UserId = userId,
                    MeetingId = meeting.Id
                };

                meetingParticipantRepository.Add(newParticipant);
                Console.WriteLine($"User {userId} added as a participant to meeting {meetingId}.");
                return true;  // Başarıyla ekleme yapılmışsa true döndür
            }
            else
            {
                Console.WriteLine($"User {userId} is already a participant in meeting {meetingId}.");
                return false;  // Kullanıcı zaten katılımcıysa false döndür
            }
        }




        public string GenerateMeetId()
        {

            // 4 karakter uzunluğunda rastgele bir string oluştur
            var part1 = GenerateRandomString(4);

            // 2 karakter uzunluğunda rastgele bir string oluştur
            var part2 = GenerateRandomString(2);

            // 3 karakter uzunluğunda rastgele bir string oluştur
            var part3 = GenerateRandomString(3);

            // Bu parçaları birleştirerek MeetId'yi oluştur
            var meetId = $"{part1}-{part2}-{part3}";

            return meetId;
        }
       
        public string GenerateRandomString(int length)
        {
            const string chars = "abcdefghıjklmnopqrstuvwxyz0123456789";

            var randomString = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());

            return randomString;
        }
    }
}
