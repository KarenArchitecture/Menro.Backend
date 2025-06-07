let cartCount = 0;

// ===== DOM References =====
const badge = document.querySelector(".cart-badge");
const modal = document.getElementById("bottom-modal");
const backdrop = document.getElementById("modal-backdrop");
const addToCartBtn = document.querySelector(".add-to-cart-btn");
const categoryButtons = document.querySelectorAll(".category-btn");
const sidebarButtons = document.querySelectorAll(".sidebar-btn");
const sizeButtons = document.querySelectorAll(".size-btn");
const addButtons = document.querySelectorAll(".add-btn");
const bottomNav = document.getElementById("bottomNav");
const toggleBar = document.querySelector(".nav-toggle-bar");
const slider = document.querySelector(".food_items"); // menu wrapper

// ===== Modal Handlers =====
function openModal() {
    modal.classList.add("active");
    backdrop.classList.add("active");
    document.body.style.overflow = "hidden";
    modal.style.bottom = navVisible ? "5rem" : "0";
}

function closeModal() {
    modal.classList.remove("active");
    backdrop.classList.remove("active");
    document.body.style.overflow = "";
    modal.style.bottom = "";
}

// ===== Cart Counter =====
function increaseCartCount() {
    cartCount++;
    badge.style.display = "flex";
    badge.textContent = cartCount > 10 ? "10+" : cartCount;
    addToCartBtn.classList.add("clicked");
    setTimeout(() => addToCartBtn.classList.remove("clicked"), 300);
}

// ===== Size‑Button Flash =====
function setupSizeButtons() {
    sizeButtons.forEach((btn) => {
        btn.addEventListener("click", () => {
            sizeButtons.forEach((b) => b.classList.remove("flash"));
            btn.classList.add("flash");
            setTimeout(() => btn.classList.remove("flash"), 1000);
        });
    });
}

// ===== Section Filter =====
// function filterSections(categoryKey) {
//   document.querySelectorAll("[data-category-section]").forEach((section) => {
//     const sec = section.dataset.categorySection;
//     const show = categoryKey === "all" || sec === categoryKey;
//     section.style.display = show ? "block" : "none";
//     section.style.pointerEvents = show ? "auto" : "none";
//   });

//   // toggle scroll direction on all food_items
//   document.querySelectorAll(".food_items").forEach((container) => {
//     if (categoryKey === "all") {
//       container.classList.add("horizontal-scroll");
//       container.classList.remove("vertical-scroll");
//     } else {
//       container.classList.remove("horizontal-scroll");
//       container.classList.add("vertical-scroll");
//     }
//   });
// }
function filterSections(categoryKey) {
    document.querySelectorAll("[data-category-section]").forEach((section) => {
        const sec = section.dataset.categorySection;
        const show = categoryKey === "all" || sec === categoryKey;

        // خودِ سکشن
        section.style.display = show ? "block" : "none";
        section.style.pointerEvents = show ? "auto" : "none";

        // عنوان + 'مشاهده همه'
        const menuNav = section.querySelector(".menu_nav");
        if (menuNav) {
            menuNav.style.display = categoryKey === "all" ? "flex" : "none";
        }
    });

    // باقی کدِ تغییر جهت اسکرول (همون قبلی)
    document.querySelectorAll(".food_items").forEach((container) => {
        if (categoryKey === "all") {
            container.classList.add("horizontal-scroll");
            container.classList.remove("vertical-scroll");
        } else {
            container.classList.remove("horizontal-scroll");
            container.classList.add("vertical-scroll");
        }
    });
}

// ===== Bottom‑Nav Toggle =====
let navVisible = true;
function toggleBottomNav() {
    navVisible = !navVisible;
    bottomNav.style.transform = navVisible ? "translateY(0)" : "translateY(100%)";
    if (modal.classList.contains("active")) {
        modal.style.bottom = navVisible ? "5rem" : "0";
    }
}

// ===== Event Wiring =====
function setupEventListeners() {
    addButtons.forEach((btn) => btn.addEventListener("click", openModal));
    backdrop.addEventListener("click", closeModal);
    addToCartBtn.addEventListener("click", increaseCartCount);
    setupSizeButtons();
    toggleBar?.addEventListener("click", toggleBottomNav);

    categoryButtons.forEach((btn) => {
        btn.addEventListener("click", () => {
            categoryButtons.forEach((b) => b.classList.remove("active"));
            btn.classList.add("active");

            sidebarButtons.forEach((b) => b.classList.remove("active"));
            const key = btn.dataset.category;
            const match = Array.from(sidebarButtons).find(
                (b) => b.dataset.cat === key
            );
            if (match) match.classList.add("active");

            filterSections(key);
        });
    });

    sidebarButtons.forEach((btn) => {
        btn.addEventListener("click", () => {
            sidebarButtons.forEach((b) => b.classList.remove("active"));
            btn.classList.add("active");

            categoryButtons.forEach((b) => b.classList.remove("active"));
            const key = btn.dataset.cat;
            const match = Array.from(categoryButtons).find(
                (b) => b.dataset.category === key
            );
            if (match) match.classList.add("active");

            filterSections(key);
        });
    });
}

// ===== Init =====
setupEventListeners();
filterSections("all");

// ===== Horizontal Scroll Limit (applies to horizontal only) =====
let isDown = false;
let startX;
let scrollLeft;

slider.addEventListener("mousedown", (e) => {
    if (!slider.classList.contains("horizontal-scroll")) return;
    isDown = true;
    slider.classList.add("dragging");
    startX = e.pageX;
    scrollLeft = slider.scrollLeft;
});

slider.addEventListener("mouseleave", () => {
    isDown = false;
    slider.classList.remove("dragging");
});

slider.addEventListener("mouseup", () => {
    isDown = false;
    slider.classList.remove("dragging");
});

slider.addEventListener("mousemove", (e) => {
    if (!isDown || !slider.classList.contains("horizontal-scroll")) return;
    e.preventDefault();
    const x = e.pageX;
    const walk = (x - startX) * 1.5;
    slider.scrollLeft = scrollLeft - walk;
});

// ===== Scroll Limit at Item 19 (only for horizontal) =====
slider.addEventListener("scroll", () => {
    if (!slider.classList.contains("horizontal-scroll")) return;
    const items = slider.querySelectorAll(".item");
    if (items.length === 0) return;

    const itemWidth = items[0].offsetWidth + 22;
    const visibleItems = Math.floor(slider.offsetWidth / itemWidth);
    const maxIndex = 19 - visibleItems;
    const maxScroll = maxIndex * itemWidth;

    if (slider.scrollLeft > maxScroll) {
        slider.scrollLeft = maxScroll;
    }
});
