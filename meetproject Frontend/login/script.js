document.addEventListener('DOMContentLoaded', () => {
    const API_BASE_URL = "http://localhost:5059"; // API'nin çalıştığı adres
    const LOGIN_API = `${API_BASE_URL}/api/User/login`;
    const REGISTER_API = `${API_BASE_URL}/api/User/register`;

    const loginForm = document.getElementById('login-form');
    const registerForm = document.getElementById('register-form');
    const loginErrorMessage = document.getElementById('login-error');
    const registerErrorMessage = document.getElementById('register-error');

    // Fonksiyonları global hale getirmek
    window.showRegister = function() {
        loginForm.style.display = 'none';
        registerForm.style.display = 'block';
    };

    window.showLogin = function() {
        registerForm.style.display = 'none';
        loginForm.style.display = 'block';
    };

    // Giriş Formu İşlemleri
    if (loginForm) {
        loginForm.addEventListener('submit', async (event) => {
            event.preventDefault();
            loginErrorMessage.style.display = "none";
        
            // Formdan e-posta ve şifreyi al
            const email = loginForm.querySelector('input[name="email"]').value;
            const password = loginForm.querySelector('input[name="password"]').value;
        
            // Alanların boş olup olmadığını kontrol et
            if (!email || !password) {
                loginErrorMessage.textContent = "E-posta ve şifre boş bırakılamaz.";
                loginErrorMessage.style.display = "block";
                return;
            }
        
            try {
                const response = await fetch(LOGIN_API, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ email, password }), // Sadece email ve password gönderiliyor
                });
    
                console.log("Response Status:", response.status);
        
                // Response kontrolü
                if (response.ok) {
                    // Response body'yi JSON olarak oku
                    const data = await response.json();
    
                    // Kullanıcı bilgilerini sessionStorage'a kaydet
                    sessionStorage.setItem("name", data.name);
                    sessionStorage.setItem("surname", data.surname);
                    sessionStorage.setItem("email", data.email);
                    sessionStorage.setItem("token", data.token); // Token'ı sakla
        
                    alert(`Giriş başarılı! Hoş geldiniz, ${data.name}`);
                    window.location.href = "../home/index.html"; // Ana sayfaya yönlendir
                } else {
                    // Hata durumunda gelen mesajı kontrol et
                    let errorMessage = "E-posta veya şifre hatalı!";
                    try {
                        const errorData = await response.json();
                        errorMessage = errorData.message || errorMessage;
                    } catch (jsonError) {
                        errorMessage = await response.text();
                    }
        
                    loginErrorMessage.textContent = errorMessage;
                    loginErrorMessage.style.display = "block";
                }
            } catch (error) {
                console.error("Fetch Error:", error);
                loginErrorMessage.textContent = "Bir hata oluştu, lütfen tekrar deneyin.";
                loginErrorMessage.style.display = "block";
            }
        });
    }

    // Kayıt Formu İşlemleri
    if (registerForm) {
        registerForm.addEventListener('submit', async (event) => {
            event.preventDefault();
            registerErrorMessage.style.display = "none";

            const name = registerForm.querySelector('input[name="name"]').value;
            const surname = registerForm.querySelector('input[name="surname"]').value;
            const email = registerForm.querySelector('input[name="email"]').value;
            const password = registerForm.querySelector('input[name="password"]').value;

            if (!name || !surname || !email || !password) {
                registerErrorMessage.textContent = "Tüm alanları doldurmanız gerekiyor!";
                registerErrorMessage.style.display = "block";
                return;
            }

            try {
                const response = await fetch(REGISTER_API, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ name, surname, email, password }),
                });

                if (response.ok) {
                    alert("Kayıt başarılı! Giriş yapabilirsiniz.");
                    showLogin(); // Kayıt sonrası login ekranına dön
                } else {
                    const errorData = await response.json();
                    registerErrorMessage.textContent =
                        errorData.errors
                            ? Object.values(errorData.errors).join(" ")
                            : errorData.message || "Kayıt sırasında hata oluştu!";
                    registerErrorMessage.style.display = "block";
                }
            } catch (error) {
                registerErrorMessage.textContent = "Bir hata oluştu, lütfen tekrar deneyin.";
                registerErrorMessage.style.display = "block";
            }
        });
    }

    // API'ye token ile erişim sağlamak için header eklemek
    async function getProtectedData() {
        const token = sessionStorage.getItem("token");
        
        if (!token) {
            alert("Token bulunamadı, giriş yapmanız gerek.");
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/api/SomeProtectedRoute`, {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`,  // Token'ı header'a ekle
                    "Content-Type": "application/json",
                },
            });

            if (response.ok) {
                const data = await response.json();
                console.log("Protected Data:", data);
            } else {
                console.error("Access Denied or Error:", response.statusText);
            }
        } catch (error) {
            console.error("Error fetching protected data:", error);
        }
    }
});
