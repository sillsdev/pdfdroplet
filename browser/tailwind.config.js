/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{ts,tsx,js,jsx}"],
  theme: {
    extend: {
      colors: {
        slate: {
          950: "#0f172a",
        },
        droplet: {
          background: "#e6e6e6",
          primary: "#1f2937",
          accent: "#2563eb",
        },
      },
      boxShadow: {
        panel: "0 10px 25px -15px rgba(15, 23, 42, 0.5)",
      },
    },
  },
  plugins: [],
};
