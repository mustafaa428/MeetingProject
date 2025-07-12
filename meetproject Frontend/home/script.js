document.addEventListener('DOMContentLoaded', () => {
  // Kullanıcı bilgileri sessionStorage'dan
  const username = sessionStorage.getItem('name');
  const surname = sessionStorage.getItem('surname');
  const email = sessionStorage.getItem('email');
  const token = sessionStorage.getItem('token');

  // DOM elemanları
  const userSection = document.getElementById('user-info');
  const loginLink = document.getElementById('login-link');
  const logoutButton = document.getElementById('logout-btn');
  const settingsLink = document.getElementById('settings-link');
  const usernameDisplay = document.getElementById('username');
  const createMeetingButton = document.getElementById('create-instant-meeting');
  const scheduleMeetingButton = document.getElementById('schedule-instant-meeting');
  const meetingTitleInput = document.getElementById('meeting-title');
  const linkContainer = document.getElementById('scheduled-meeting-link-container');

  // Menü durumunu ayarla
  if (username && surname && email && token) {
    usernameDisplay.textContent = `${username} ${surname}`;
    userSection.style.display = 'block';
    loginLink.style.display = 'none';
    settingsLink.style.display = 'inline';
  } else {
    userSection.style.display = 'none';
    loginLink.style.display = 'block';
    settingsLink.style.display = 'none';
  }

  // Çıkış işlemi
  logoutButton.addEventListener('click', () => {
    sessionStorage.clear();
    alert('Oturum kapatıldı!');
    window.location.href = '../home/index.html';
  });

  // Tarih ve saati göster
  function displayDateTime() {
    const now = new Date();
    const calendar = document.getElementById('system-calendar');
    const time = document.getElementById('system-time');

    calendar.value = now.toISOString().split('T')[0];
    time.value = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
  displayDateTime();

  // "Şimdi toplantı oluştur" butonu - mevcut işlev
  createMeetingButton.addEventListener('click', async () => {
    const title = meetingTitleInput.value.trim();
    if (!title) {
      alert('Lütfen toplantı başlığı giriniz.');
      return;
    }

    const url = 'http://localhost:5059/api/Meet/Create';
    const meetingData = { title };

    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(meetingData),
      });

      if (response.ok) {
        const data = await response.json();

        if (data.meetingId) {
          alert('Toplantı oluşturuldu!');
          sessionStorage.setItem('meetingId', data.meetingId);
          window.location.href = `../meet/index.html?meetingId=${data.meetingId}`;
        } else {
          alert('Toplantı ID alınamadı!');
        }
      } else {
        alert('Toplantı oluşturulamadı! Hata kodu: ' + response.status);
      }
    } catch (error) {
      console.error('Hata:', error);
      alert('Bir hata oluştu! Lütfen tekrar deneyiniz.');
    }
  });

  // "Toplantı planla" butonu - yeni işlev: sadece link gösteren, kapatılabilir readonly input
  scheduleMeetingButton.addEventListener('click', async () => {
    const title = meetingTitleInput.value.trim();

    // Önce alanı temizle ve gizle
    linkContainer.innerHTML = '';
    linkContainer.style.display = 'none';

    if (!title) {
      alert('Lütfen toplantı başlığı giriniz.');
      return;
    }

    scheduleMeetingButton.disabled = true;
    meetingTitleInput.disabled = true;

    try {
      const response = await fetch('http://localhost:5059/api/Meet/Create', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({ title }),
      });

      if (response.ok) {
        const data = await response.json();

        if (data.meetingId) {
          const meetingLink = `${window.location.origin}/meet/index.html?meetingId=${data.meetingId}`;

          // Kapatma butonu
          const closeBtn = document.createElement('button');
          closeBtn.textContent = '×';
          closeBtn.title = 'Kapat';
          Object.assign(closeBtn.style, {
            position: 'absolute',
            top: '5px',
            right: '10px',
            background: 'transparent',
            border: 'none',
            fontSize: '1.5rem',
            cursor: 'pointer',
            lineHeight: '1',
            padding: '0',
          });
          closeBtn.addEventListener('click', () => {
            linkContainer.style.display = 'none';
          });

          // Readonly input
          const input = document.createElement('input');
          input.type = 'text';
          input.className = 'form-control';
          input.readOnly = true;
          input.value = meetingLink;
          input.style.paddingRight = '2rem';
          input.addEventListener('click', () => input.select());

          // Container stil ve pozisyon ayarları
          linkContainer.style.position = 'relative';
          linkContainer.style.padding = '1rem';
          linkContainer.style.border = '1px solid #ccc';
          linkContainer.style.borderRadius = '5px';
          linkContainer.style.backgroundColor = '#f8f9fa';
          linkContainer.style.display = 'block';

          linkContainer.appendChild(closeBtn);
          linkContainer.appendChild(input);
        } else {
          showError('Toplantı oluşturulamadı');
        }
      } else {
        showError('Toplantı oluşturulamadı');
      }
    } catch (error) {
      console.error(error);
      showError('Toplantı oluşturulamadı');
    } finally {
      scheduleMeetingButton.disabled = false;
      meetingTitleInput.disabled = false;
    }
  });

  // Hata mesajını gösteren fonksiyon
  function showError(message) {
    linkContainer.style.display = 'block';
    linkContainer.style.position = 'relative';
    linkContainer.style.padding = '1rem';
    linkContainer.style.border = '1px solid red';
    linkContainer.style.borderRadius = '5px';
    linkContainer.style.backgroundColor = '#ffe6e6';
    linkContainer.style.color = 'red';
    linkContainer.textContent = message;
  }
});
