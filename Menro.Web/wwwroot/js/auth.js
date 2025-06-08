// login page
const loginForm = document.getElementById('loginForm');
if (loginForm) {
    loginForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        // پاک کردن پیام قبلی
        //const messageDiv = document.getElementById('message');
        //messageDiv.innerText = '';
        //messageDiv.style.display = 'none';

        // دریافت مقادیر
        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;

        // ارسال درخواست لاگین
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        const result = await response.json();

        if (response.ok) {
            localStorage.setItem('token', result.token); // ذخیره سازی توکن
            window.location.href = '/index.html'; // صفحه بعد از لاگین
        } else {
            messageDiv.innerText = 'Login failed';
            messageDiv.style.display = 'block';
        }
    });
}

// register page
const registerForm = document.getElementById('registerForm');
if (registerForm) {
    registerForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        // پاک کردن پیام قبلی
        const messageDiv = document.getElementById('message');
        messageDiv.innerText = '';
        messageDiv.style.display = 'none';

        const fullName = document.getElementById('fullName').value.trim();
        const email = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        // بررسی مطابقت پسورد
        if (password !== confirmPassword) {
            messageDiv.innerText = 'Password and Confirm Password do not match.';
            messageDiv.style.display = 'block';
            return;
        }

        try {
            const response = await fetch('/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ fullName, email, password })
            });

            const result = await response.json();

            if (response.ok) {
                window.location.href = '/login.html';
            } else {
                messageDiv.innerText = result.message || 'Registration failed';
                messageDiv.style.display = 'block';
            }
        } catch (error) {
            messageDiv.innerText = 'An error occurred. Please try again.';
            messageDiv.style.display = 'block';
            console.error('Registration error:', error);
        }
    });
}

// logout
const logoutBtn = document.getElementById('logoutBtn');

if (logoutBtn) {
    logoutBtn.addEventListener('click', function () {
        // پاک کردن توکن
        localStorage.removeItem('token');

        // اگر چیز دیگه‌ای ذخیره کردی اینجا پاک کن
        // localStorage.removeItem('user');
        // sessionStorage.clear(); ← فقط اگه استفاده کرده بودی

        // هدایت به صفحه لاگین
        window.location.href = '/login.html';
    });
}
