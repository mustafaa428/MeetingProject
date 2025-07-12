document.addEventListener('DOMContentLoaded', () => {
    const API = 'http://localhost:5059/api/User/tokenInfo'; // Kullanıcı bilgilerini çekmek için API
    const updateAPI = 'http://localhost:5059/api/User/update'; // Güncelleme için API
    const token = sessionStorage.getItem('token'); // Token'ı sessionStorage'dan al

    if (!token) {
        alert("Lütfen giriş yapın.");
        window.location.href = "/login"; // Eğer token yoksa login sayfasına yönlendir
    }
    console.log("Authorization Header:", `Bearer ${token}`);

    // Mevcut kullanıcı bilgilerini çek ve formu doldur
    fetch(API, {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error("Kullanıcı bilgileri alınamadı");
        }
        return response.json();
    })
    .then(user => {
        // Form alanlarını doldur
        document.getElementById("email").value = user.email || "";
        document.getElementById("name").value = user.name || "";
        document.getElementById("surname").value = user.surname || "";
    })
    .catch(error => {
        console.error("Kullanıcı bilgileri alınamadı:", error);
        showMessage("Kullanıcı bilgileri alınamadı!", "error");
    });

    // Ad güncelleme
    document.getElementById("updateNameBtn").addEventListener("click", () => {
        const name = document.getElementById("name").value;
        if (!name) {
            showMessage("Ad boş olamaz!", "error");
            return;
        }
        updateUserField({ Name: name });
    });

    // Soyad güncelleme
    document.getElementById("updateSurnameBtn").addEventListener("click", () => {
        const surname = document.getElementById("surname").value;
        if (!surname) {
            showMessage("Soyad boş olamaz!", "error");
            return;
        }
        updateUserField({ Surname: surname });
    });

    // E-posta güncelleme
    document.getElementById("updateEmailBtn").addEventListener("click", () => {
        const email = document.getElementById("email").value;
        if (!email) {
            showMessage("E-posta boş olamaz!", "error");
            return;
        }
        updateUserField({ Email: email });
    });

    // Şifre güncelleme
    document.getElementById("updatePasswordBtn").addEventListener("click", () => {
        const password = document.getElementById("password").value;
        const confirmPassword = document.getElementById("confirmPassword").value;

        if (!password) {
            showMessage("Şifre boş olamaz!", "error");
            return;
        }

        if (password !== confirmPassword) {
            showMessage("Şifreler uyuşmuyor!", "error");
            return;
        }

        updateUserField({ Password: password });
    });

    // API'ye güncelleme isteği gönderme
    function updateUserField(updatedField) {
        fetch(updateAPI, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(updatedField)
        })
        .then(response => response.json())
        .then(data => {
            handleResponse(data);
        })
        .catch(error => {
            showMessage("Bir hata oluştu!", "error");
        });
    }

    // API'den gelen cevabı işleme
    function handleResponse(data) {
        if (data.message) {
            showMessage(data.message, "success");
        } else {
            showMessage("Güncelleme sırasında bir hata oluştu!", "error");
        }
    }

    // Mesaj gösterme
    function showMessage(message, type) {
        const messageElement = document.getElementById("message");
        messageElement.textContent = message;
        messageElement.classList.remove("success", "error");
        messageElement.classList.add(type);
    }
});
