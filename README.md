# 📡 MeetingApp

MeetingApp is a simple web-based meeting application that allows users to quickly create meetings with JWT authentication. After logging in, users can create instant meetings and view the generated meeting link.

## 🚀 Running the Application

Start the ASP.NET Core Web API project using Visual Studio or the .NET CLI:
dotnet run

Open the frontend by launching the `index.html` file using **Live Server** in Visual Studio Code.

Enter a meeting title and click the "Create Instant Meeting" or "Schedule Meeting" button. The backend will generate a `meetId` in the format of `xxx-yy-zzz`.

This link can be shared with participants.

If the participant has a valid token, they will be added to the participants table in the database.

Real-time messaging and video communication are handled on the backend using **SignalR**.

## ⚙️ appsettings.json Configuration

The following configuration must be added to the `appsettings.json` file on the backend for JWT authentication and database connection:

```json
{
  "Jwt": {
    "Key": "",
    "Issuer": "",
    "Audience": "",
    "ExpireMinutes": 
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MeetingDb;Trusted_Connection=True;"
  }
}
Key: The secret key used to sign the JWT token.

Issuer / Audience: The domain issuing the token and where it's valid.

ExpireMinutes: Token expiration time in minutes.

DefaultConnection: Connection string for the MSSQL database.

⚠️ Warning: In production environments, secret keys should not be hardcoded. Use environment variables or secure configuration management instead.

🧪 Test Summary
Meetings cannot be created without a valid token.

After a successful request, the user is redirected to the meeting room or the generated link is displayed on screen.

The frontend is built entirely with HTML, CSS, and  JavaScript.
The backend is developed with ASP.NET.



# 📡 MeetingApp

MeetingApp, kullanıcıların JWT doğrulaması ile web üzerinden hızlıca toplantı oluşturmasını sağlayan basit bir toplantı uygulamasıdır. Kullanıcılar giriş yaptıktan sonra anlık toplantı oluşturabilir ve oluşturulan bağlantıyı görüntüleyebilir.

## 🚀 Uygulamanın Çalıştırılması

ASP.NET Core Web API projenizi Visual Studio veya .NET CLI ile başlatın:
dotnet run

Frontend tarafını `index.html` dosyasını visual studio code üzerinden open live server kullanarak açın.


Toplantı başlığı girerek "Şimdi Toplantı Oluştur" veya "Toplantı Planla" butonuna tıklayın. Backend tarafında bir tane xxx-yy-zzz şeklinde bir meetid oluşturur

katılımcılar ile link paylaşılır.

katılımcı token sahibi ise veri tabanındaki katılımcı tablosuna eklenir.

anlık mesajlaşma ve görüntülü sohbet backend tarafında signalR ile sağlanmaktadır.

⚙️ appsettings.json Yapılandırması
Backend tarafında JWT kimlik doğrulaması ve veritabanı bağlantısı için aşağıdaki yapı appsettings.json dosyasına eklenmelidir:

{
  "Jwt": {
    "Key": "",
    "Issuer": "",
    "Audience": "",
    "ExpireMinutes": 
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MeetingDb;Trusted_Connection=True;"
  }
}
Key: JWT token imzalamak için kullanılan gizli anahtar.

Issuer / Audience: Token’ın üretildiği ve geçerli olduğu alan adları.

ExpireMinutes: Token süresi (dakika cinsinden).

DefaultConnection: MSSQL veritabanı bağlantı dizesi.

⚠️ Uyarı: Gerçek projelerde gizli anahtarlar açıkta tutulmamalı; environment değişkenleri veya gizli yapılandırmalar kullanılmalıdır.

🧪 Test Özeti
Token olmadan toplantı oluşturulamaz.

Başarılı bir istekten sonra kullanıcı toplantı odasına yönlendirilir veya bağlantı ekranda görünür.

Frontend  HTML, CSS ve JavaScript ile hazırlanmıştır.
Backend ASP.NET ile geliştirilmiştir.
