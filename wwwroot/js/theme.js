window.themeManager = {
    setTheme: function (isDark) {
        if (isDark) {
            document.documentElement.classList.add('dark');
            localStorage.setItem('theme', 'dark');
        } else {
            document.documentElement.classList.remove('dark');
            localStorage.setItem('theme', 'light');
        }
    },
    getTheme: function () {
        var theme = localStorage.getItem('theme');
        if (!theme) {
            theme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        }
        return theme === 'dark';
    },
    initTheme: function () {
        var theme = localStorage.getItem('theme');
        var isDark = false;
        if (theme) {
            isDark = theme === 'dark';
        } else {
            isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        }
        if (isDark) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
    }
};

// Auto-run theme initialization as early as possible
themeManager.initTheme();
