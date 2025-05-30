document.addEventListener("DOMContentLoaded", function () {
  const searchInput = document.getElementById("searchInput");
  const searchButton = document.getElementById("searchButton");
  const modal = document.getElementById("searchModal");
  const overlay = document.getElementById("modalOverlay");
  const searchBar = document.querySelector(".search-bar");

  function showModal() {
    modal.classList.add("active");
    overlay.classList.add("active");
    searchBar.classList.add("hidden");
  }

  function hideModal() {
    modal.classList.remove("active");
    overlay.classList.remove("active");
    searchBar.classList.remove("hidden");
  }

  searchButton.addEventListener("click", function (e) {
    e.preventDefault();
    showModal();
  });

  searchInput.addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
      e.preventDefault();
      showModal();
    }
  });

  // ✅ This is the click-outside-to-close behavior
  overlay.addEventListener("click", hideModal);
});
