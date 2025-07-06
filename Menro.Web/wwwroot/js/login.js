// otp service (سازگار با فرم loginFormOtp)
document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("loginFormOtp");
    const phoneInput = document.getElementById("phone");
    const codeGroup = document.getElementById("code-group");
    const codeInput = document.getElementById("code");
    const submitButton = document.getElementById("submitButton");
    const messageArea = document.getElementById("message-area");

    if (!form) return; // اگر فرم OTP نیست، خروج

    let otpSent = false;

    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        messageArea.textContent = "";
        messageArea.className = "message error";

        const phoneNumber = phoneInput.value.trim();
        if (!/^\d{11}$/.test(phoneNumber)) {
            displayMessage("لطفاً یک شماره تلفن ۱۱ رقمی معتبر وارد کنید", "error");
            return;
        }

        if (!otpSent) {
            // مرحله اول: ارسال کد تایید
            try {
                const response = await fetch("/api/auth/send-otp", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ phoneNumber }),
                });

                if (response.ok) {
                    otpSent = true;
                    codeGroup.style.display = "block";
                    phoneInput.readOnly = true;
                    submitButton.textContent = "تایید کد";
                    displayMessage("کد تأیید ارسال شد.", "success");
                    codeInput.focus();
                } else {
                    const data = await response.json();
                    displayMessage(data.message || "خطا در ارسال کد.");
                }
            } catch (error) {
                displayMessage("خطا در ارتباط با سرور.");
            }
        }

        else {
            // مرحله دوم: بررسی کد تایید و ورود یا هدایت به ثبت‌نام
            const code = codeInput.value.trim();

            if (code.length !== 4) {
                displayMessage("کد باید ۴ رقم باشد.");
                return;
            }

            try {
                const response = await fetch("/api/auth/verify-otp", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ phoneNumber, code }),
                });

                const result = await response.json();

                if (response.ok) {
                    if (result.needsRegister) {
                        localStorage.setItem("pendingPhoneNumber", phoneNumber);
                        window.location.href = "/pages/auth/register.html";
                    } else {
                        localStorage.setItem("token", result.token);
                        window.location.href = "/Home/Index";
                    }
                } else {
                    displayMessage(result.message || "کد وارد شده صحیح نیست.");
                }
            } catch (error) {
                displayMessage("خطا در ارتباط با سرور.");
            }
        }
    });

    function displayMessage(text, type = "error") {
        messageArea.textContent = text;
        messageArea.className = `message ${type}`;
    }
});


// سوییچ بین تب‌ها
document.addEventListener("DOMContentLoaded", () => {
    const otpTab = document.getElementById("otp-tab");
    const passwordTab = document.getElementById("password-tab");
    const formOtp = document.getElementById("loginFormOtp");
    const formPass = document.getElementById("loginFormPassword");

    otpTab.addEventListener("click", () => {
        otpTab.classList.add("active");
        passwordTab.classList.remove("active");
        formOtp.style.display = "block";
        formPass.style.display = "none";
    });

    passwordTab.addEventListener("click", () => {
        passwordTab.classList.add("active");
        otpTab.classList.remove("active");
        formPass.style.display = "block";
        formOtp.style.display = "none";
    });
});