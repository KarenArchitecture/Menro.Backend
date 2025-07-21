//const { get } = require("jquery");

document.addEventListener("DOMContentLoaded", () => {
    // Authorization Control
    token = localStorage.getItem("token");

    if (!token) {
        // توکن وجود نداره، کاربر رو به صفحه لاگین منتقل کن
        window.location.href = "/pages/login.html";
    }

    const ownerForm = document.getElementById("owner-form");
    const messageArea = document.getElementById("message-area");
    const categorySelect = document.getElementById("restaurant-category");

    // اعمال Choices.js به select با جستجو
    const choices = new Choices(categorySelect, {
        searchEnabled: true,
        itemSelectText: "",
        shouldSort: false,
        placeholder: true,
        searchPlaceholderValue: "جستجو کنید...",
    });

    // گرفتن دسته‌بندی‌ها از سرور
    fetch("/api/restaurants/categories")
        .then((res) => {
            if (!res.ok) throw new Error("خطا در دریافت دسته‌بندی‌ها");
            return res.json();
        })
        .then((categories) => {
            const options = categories.map((cat) => ({
                value: cat.id,
                label: cat.name,
            }));
            choices.setChoices(options, "value", "label", true);
        })
        .catch((err) => {
            console.error("خطا:", err);
            displayMessage("خطا در دریافت دسته‌بندی‌ها", "error");
        });

    // مدیریت ارسال فرم رستوران
    ownerForm.addEventListener("submit", async (event) => {
        event.preventDefault();
        clearMessage();

        // اعتبارسنجی زمان
        const openTimeRaw = document.getElementById("restaurant-open-time").value;
        const closeTimeRaw = document.getElementById("restaurant-close-time").value;

        if (!isTimeValid(openTimeRaw, closeTimeRaw)) {
            displayMessage("ساعت پایان باید بعد از ساعت شروع باشد.", "error");
            return;
        }

        // اطمینان از اینکه زمان به صورت hh:mm:ss فرستاده میشه
        const openTime = openTimeRaw.length === 5 ? openTimeRaw + ":00" : openTimeRaw;
        const closeTime = closeTimeRaw.length === 5 ? closeTimeRaw + ":00" : closeTimeRaw;

        const restaurantData = {
            restaurantName: document.getElementById("restaurant-name").value,
            restaurantDescription: document.getElementById("restaurant-description").value,
            restaurantAddress: document.getElementById("restaurant-address").value,
            restaurantCategoryId: parseInt(categorySelect.value),
            restaurantOpenTime: openTime,
            restaurantCloseTime: closeTime,
            ownerNationalId: document.getElementById("owner-national-id").value,
            restaurantAccountNumber: document.getElementById("restaurant-account-number").value
        };

        console.log("اطلاعات رستوران ارسال شد:", restaurantData);

        try {
            const response = await fetch("/api/restaurants/register", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": "Bearer " + localStorage.getItem("token")
                },
                body: JSON.stringify(restaurantData),
            });

            const result = await response.text(); // یا await response.json() اگر خروجی JSON باشه

            if (response.ok) {
                displayMessage("رستوران با موفقیت ثبت شد", "success");
                console.log("پاسخ سرور:", result);
                window.location.href = "/Home/Index";
            }
            else {
                displayMessage("ثبت رستوران با مشکل مواجه شد", "error");
                console.warn("خطای ثبت:", result);
            }
        } catch (error) {
            console.error("خطای ارتباط با سرور:", error);
            displayMessage("خطا در ثبت رستوران", "error");
        }
    });


    function isTimeValid(start, end) {
        const [startH, startM] = start.split(":").map(Number);
        const [endH, endM] = end.split(":").map(Number);
        const startMinutes = startH * 60 + startM;
        const endMinutes = endH * 60 + endM;
        return endMinutes > startMinutes;
    }

    function displayMessage(text, type) {
        messageArea.textContent = text;
        messageArea.className = `message ${type}`;
    }

    function clearMessage() {
        messageArea.textContent = "";
        messageArea.className = "message";
    }

});
