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

// ===== Modal Handlers =====
function openModal() {
  modal.classList.add("active");
  backdrop.classList.add("active");
  document.body.style.overflow = "hidden";
  // if bottom nav is visible, push modal above it
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
function filterSections(categoryKey) {
  document.querySelectorAll("[data-category-section]").forEach((section) => {
    const sec = section.dataset.categorySection;
    const show = categoryKey === "all" || sec === categoryKey;
    section.style.display = show ? "block" : "none";
    section.style.pointerEvents = show ? "auto" : "none";
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
  // 1) Open‑modal on every “+” button
  addButtons.forEach((btn) => btn.addEventListener("click", openModal));
  // 2) Click outside to close
  backdrop.addEventListener("click", closeModal);
  // 3) Cart++
  addToCartBtn.addEventListener("click", increaseCartCount);
  // 4) Size flash
  setupSizeButtons();
  // 5) Bottom‑nav toggle
  toggleBar?.addEventListener("click", toggleBottomNav);

  // 6) Mobile horizontal category pills
  categoryButtons.forEach((btn) => {
    btn.addEventListener("click", () => {
      categoryButtons.forEach((b) => b.classList.remove("active"));
      btn.classList.add("active");
      // sync desktop
      sidebarButtons.forEach((b) => b.classList.remove("active"));
      const key = btn.dataset.category;
      const match = Array.from(sidebarButtons).find(
        (b) => b.dataset.cat === key
      );
      if (match) match.classList.add("active");
      filterSections(key);
    });
  });

  // 7) Desktop vertical sidebar
  sidebarButtons.forEach((btn) => {
    btn.addEventListener("click", () => {
      sidebarButtons.forEach((b) => b.classList.remove("active"));
      btn.classList.add("active");
      // sync mobile
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
// show “all” by default
filterSections("all");







    const slider = document.querySelector('.food_items');
    let isDown = false;
    let startX;
    let scrollLeft;

    slider.addEventListener('mousedown', (e) => {
    isDown = true;
    slider.classList.add('dragging');
    startX = e.pageX;
    scrollLeft = slider.scrollLeft;
    });

    slider.addEventListener('mouseleave', () => {
    isDown = false;
    slider.classList.remove('dragging');
    });

    slider.addEventListener('mouseup', () => {
    isDown = false;
    slider.classList.remove('dragging');
    });

    slider.addEventListener('mousemove', (e) => {
    if (!isDown) return;
    e.preventDefault(); // Prevent selection while dragging
    const x = e.pageX;
    const walk = (x - startX) * 1.5; // Increase this multiplier for faster drag
    slider.scrollLeft = scrollLeft - walk;
    });