/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    "./Components/**/*.razor",
    "./Components/Pages/**/*.razor",
    "./Components/Layout/**/*.razor",
    "./wwwroot/**/*.html",
    "./wwwroot/**/*.js",
    "./App.razor"
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Outfit', 'Inter', 'system-ui', '-apple-system', 'sans-serif'],
        mono: ['Fira Code', 'JetBrains Mono', 'monospace'],
      },
    },
  },
  plugins: [],
}
