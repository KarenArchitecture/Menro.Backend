//ADD / REDUCE ITEM
document.addEventListener("DOMContentLoaded", function () {
  const qtyControllers = document.querySelectorAll(".qty-controller");

  qtyControllers.forEach((controller) => {
    const plusBtn = controller.querySelector(".plus");
    const minusBtn = controller.querySelector(".minus");
    const valueBox = controller.querySelector(".qty-value");

    plusBtn.addEventListener("click", () => {
      let current = parseInt(valueBox.textContent);
      valueBox.textContent = current + 1;
    });

    minusBtn.addEventListener("click", () => {
      let current = parseInt(valueBox.textContent);
      if (current > 1) valueBox.textContent = current - 1;
    });
  });
});

document.getElementById("payBtn").addEventListener("click", function () {
  // Change button text
  this.innerText = "روش‌های پرداخت";

  // Show modal & overlay
  document.getElementById("paymentModal").classList.add("active");
  document.getElementById("modalOverlay").classList.add("active");

  // Push footer up
  document.getElementById("checkoutFooter").classList.add("footer-pushed");
});

// Close modal when clicking the overlay
document.getElementById("modalOverlay").addEventListener("click", function () {
  // Reset button text
  document.getElementById("payBtn").innerText = "پرداخت";

  // Hide modal & overlay
  this.classList.remove("active");
  document.getElementById("paymentModal").classList.remove("active");

  // Reset footer position
  document.getElementById("checkoutFooter").classList.remove("footer-pushed");
});
