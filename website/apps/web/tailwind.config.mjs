/** @type {import('tailwindcss').Config} */
export default {
  content: ['./src/**/*.{astro,html,js,jsx,md,mdx,svelte,ts,tsx,vue}'],
  theme: {
    extend: {
      colors: {
        terminal: {
          bg: '#1e1e1e',
          fg: '#cccccc',
          cursor: '#ffffff',
          selection: '#264f78'
        }
      }
    },
  },
  plugins: [],
}
