document.addEventListener("DOMContentLoaded", () => {
    const logoutBtn = document.getElementById("logoutBtn");

    if (logoutBtn) {
        logoutBtn.addEventListener("click", (e) => {
            e.preventDefault();

            // پاک کردن توکن و اطلاعات کاربر از localStorage
            localStorage.removeItem("token");
            localStorage.removeItem("user");
            localStorage.removeItem("userPhone");
            window.location.href = "/Home/Index";

            // (اختیاری) تماس با API برای ثبت لاگ خروج
            //fetch("/api/auth/logout", {
            //    method: "POST"
            //}).finally(() => {
            //    // ریدایرکت به صفحه اصلی
            //    window.location.href = "/Home/Index";
            //});
        });
    }
});
