// =========================================================
// WORKSPHERE THEME + MOBILE NAVIGATION
// =========================================================

document.addEventListener("DOMContentLoaded", function () {

    // =====================================================
    // DARK MODE
    // =====================================================

    const themeButton = document.querySelector(".notification-button");

    if (themeButton) {

        // Check if the user previously selected dark mode
        const savedTheme = localStorage.getItem("worksphere-theme");

        if (savedTheme === "dark") {
            document.body.classList.add("dark-mode");
            themeButton.textContent = "☀";
        }


        // Toggle theme
        themeButton.addEventListener("click", function () {

            document.body.classList.toggle("dark-mode");

            const isDarkMode =
                document.body.classList.contains("dark-mode");

            if (isDarkMode) {

                localStorage.setItem("worksphere-theme", "dark");

                themeButton.textContent = "☀";

            } else {

                localStorage.setItem("worksphere-theme", "light");

                themeButton.textContent = "☾";

            }

        });

    }


    // =====================================================
    // MOBILE SIDEBAR
    // =====================================================

    const menuButton = document.getElementById("mobileMenuButton");
    const sidebar = document.getElementById("sidebar");
    const closeButton = document.getElementById("sidebarCloseButton");

    if (menuButton && sidebar) {

        menuButton.addEventListener("click", function () {

            sidebar.classList.add("mobile-open");

            menuButton.setAttribute("aria-expanded", "true");

        });


        if (closeButton) {

            closeButton.addEventListener("click", function () {

                sidebar.classList.remove("mobile-open");

                menuButton.setAttribute("aria-expanded", "false");

            });

        }

    }

});