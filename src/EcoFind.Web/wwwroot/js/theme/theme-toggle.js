(function toggleTheme() {
    const themeToggle = document.getElementById('themeToggle');
    const html = document.documentElement;
    const currentTheme = localStorage.getItem('theme') || 'light';
    html.setAttribute('data-bs-theme', currentTheme);
    updateThemeIcons(currentTheme);

    themeToggle.addEventListener('click', function (event) {
        event.stopPropagation();
        const currentTheme = html.getAttribute('data-bs-theme');
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';

        html.setAttribute('data-bs-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        updateThemeIcons(newTheme);
    });
})();