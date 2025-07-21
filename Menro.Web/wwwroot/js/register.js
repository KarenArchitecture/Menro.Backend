// گرفتن و پاک کردن شماره تلفن از local storage
document.addEventListener("DOMContentLoaded", () => {
    const phoneInput = document.getElementById("customer-phoneNumber");
    const userPhoneRaw = localStorage.getItem("userPhone");

    if (!userPhoneRaw) {
        window.location.href = "/pages/auth/login.html";
        return;
    }

    const userPhone = JSON.parse(userPhoneRaw);

    if (!userPhone.value || Date.now() > userPhone.expiresAt) {
        localStorage.removeItem("userPhone");
        window.location.href = "/pages/auth/login.html";
        return;
    }

    phoneInput.value = userPhone.value;
    phoneInput.readOnly = true;
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
                localStorage.removeItem('userPhone');
                window.location.href = "/Home/Index";
            } else {
                messageArea.textContent = result.message || "ثبت‌نام ناموفق بود";
            }
        } catch (error) {
            messageArea.textContent = "خطا در ارتباط با سرور.";
            console.error(error);
        }
    });
}
