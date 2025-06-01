/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,html,cs}",
    "./Services/**/*.cs",
    "./Models/**/*.cs",
    "./Data/**/*.cs",
    "./Pages/**/*.{razor,html,cs}",
    "./*.razor"
  ],
  theme: {
    extend: {},
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
} 