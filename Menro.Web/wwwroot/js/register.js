// گرفتن و پاک کردن شماره تلفن از local storage
document.addEventListener("DOMContentLoaded", () => {
    const phoneInput = document.getElementById("customer-phoneNumber");
    const phoneNumber = localStorage.getItem("pendingPhoneNumber");

    if (phoneInput && phoneNumber) {
        phoneInput.value = phoneNumber;
        phoneInput.readOnly = true;
    } else {
        // هدایت مجدد به صفحه لاگین اگه مستقیم بدون شماره تلفن وارد صفحه ثبت نام شده بود
        window.location.href = "/pages/auth/login.html";
    }
});

const customerForm = document.getElementById("customer-form");

if (customerForm) {
    customerForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const messageArea = document.getElementById("message-area");
        messageArea.textContent = "";
        messageArea.style.color = "red";

        // گرفتن مقادیر فرم
        const fullName = document.getElementById("customer-name").value.trim();
        const phoneNumber = document.getElementById("customer-phoneNumber").value.trim();
        const email = document.getElementById("customer-email").value.trim();
        const password = document.getElementById("customer-password").value;

        // اعتبارسنجی اولیه
        if (!fullName) {
            messageArea.textContent = "وارد کردن نام الزامی است";
            return;
        }

        try {
            const response = await fetch("/api/auth/register", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ fullName, phoneNumber, email, password }),
            });

            const result = await response.json();

            if (response.ok) {
                // ذخیره توکن و حذف شماره تلفن
                localStorage.setItem("token", result.token);
                localStorage.removeItem('pendingPhoneNumber');
                window.location.href = '/index.html';
            } else {
                messageArea.textContent = result.message || "ثبت‌نام ناموفق بود";
            }
        } catch (error) {
            messageArea.textContent = "خطا در ارتباط با سرور.";
            console.error(error);
        }
    });
}
