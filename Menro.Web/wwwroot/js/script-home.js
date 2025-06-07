// mobile navigation
document.addEventListener("DOMContentLoaded", () => {
    const navItems = document.querySelectorAll(".nav-item");

    navItems.forEach((item) => {
        item.addEventListener("click", () => {
            navItems.forEach((nav) => nav.classList.remove("active"));
            item.classList.add("active");
        });
    });
});


//carousel
const carousel = document.querySelector(".carousel-slider");
const indicators = document.querySelectorAll(".indicator");
let currentIndex = 0;
let startX = 0;
let isDragging = false;
let moveX = 0;
let autoSlideInterval;

const isRTL = document.documentElement.dir === "rtl";

function startAutoSlide() {
    autoSlideInterval = setInterval(() => {
        currentIndex = currentIndex < indicators.length - 1 ? currentIndex + 1 : 0;
        updateCarousel();
    }, 5000); // تغییر اسلاید هر ۳ ثانیه
}

function stopAutoSlide() {
    clearInterval(autoSlideInterval);
}

function updateCarousel() {
    carousel.style.transform = `translateX(${currentIndex * 100}%)`;
    indicators.forEach((ind, index) => {
        ind.classList.toggle("active", index === currentIndex);
    });
}

function handleTouchStart(event) {
    isDragging = true;
    startX = event.touches ? event.touches[0].clientX : event.clientX;
}

function handleTouchMove(event) {
    if (!isDragging) return;
    moveX = event.touches ? event.touches[0].clientX : event.clientX;
}

function handleTouchEnd() {
    if (!isDragging) return;
    let diff = startX - moveX;
    if ((isRTL && diff < -50) || (!isRTL && diff > 50)) {
        currentIndex = currentIndex < indicators.length - 1 ? currentIndex + 1 : 0;
    } else if ((isRTL && diff > 50) || (!isRTL && diff < -50)) {
        currentIndex = currentIndex > 0 ? currentIndex - 1 : indicators.length - 1;
    }
    updateCarousel();
    isDragging = false;
}

document.querySelectorAll(".carousel-slider img").forEach((img) => {
    img.addEventListener("dragstart", (event) => event.preventDefault());
});

indicators.forEach((indicator) => {
    indicator.addEventListener("click", () => {
        currentIndex = parseInt(indicator.dataset.index);
        updateCarousel();
    });
});
// راه‌اندازی تایمر خودکار
startAutoSlide();

// وقتی کاربر ماوس یا لمس می‌کند، تایمر متوقف شود
carousel.addEventListener("mouseenter", stopAutoSlide);
carousel.addEventListener("mouseleave", startAutoSlide);
carousel.addEventListener("touchstart", stopAutoSlide);
carousel.addEventListener("touchend", startAutoSlide);
carousel.addEventListener("mousedown", handleTouchStart);
carousel.addEventListener("mousemove", handleTouchMove);
carousel.addEventListener("mouseup", handleTouchEnd);
carousel.addEventListener("mouseleave", handleTouchEnd);
carousel.addEventListener("touchstart", handleTouchStart);
carousel.addEventListener("touchmove", handleTouchMove);
carousel.addEventListener("touchend", handleTouchEnd);
