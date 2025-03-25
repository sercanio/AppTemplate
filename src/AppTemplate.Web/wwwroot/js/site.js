function updateThemeIcons(theme) {
    const sunIcon = themeToggle.querySelector('.theme-icon-light');
    const moonIcon = themeToggle.querySelector('.theme-icon-dark');
    if (theme === 'dark') {
        sunIcon.classList.add('d-none');
        moonIcon.classList.remove('d-none');
    } else {
        sunIcon.classList.remove('d-none');
        moonIcon.classList.add('d-none');
    }
}

(function () {
    const theme = localStorage.getItem('theme') || 'light';
    const html = document.documentElement;
    html.setAttribute('data-bs-theme', theme);
    const themeToggle = document.getElementById('themeToggle');
    if (themeToggle) {
        updateThemeIcons(theme);
    }
})();