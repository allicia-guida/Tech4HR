export type ThemeColors = {
  background: string;
  surface: string;
  surfaceSecondary: string;
  text: string;
  textSecondary: string;
  border: string;
  primary: string;
  primaryPressed: string;
  success: string;
  danger: string;
  onPrimary: string;
  overlay: string;
};

// A paleta é a mesma do site (Tech4Hr.Web, wwwroot/css/tech4hr.css), para o
// aplicativo e o site parecerem um produto só.
export const lightColors: ThemeColors = {
  background: "#F3F6FB",
  surface: "#FFFFFF",
  surfaceSecondary: "#F1F5F9",
  text: "#0F172A",
  textSecondary: "#64748B",
  border: "#E2E8F0",
  primary: "#2563EB",
  primaryPressed: "#1D4ED8",
  success: "#15803D",
  danger: "#B91C1C",
  onPrimary: "#FFFFFF",
  overlay: "rgba(15,23,42,0.48)",
};

export const darkColors: ThemeColors = {
  background: "#0B1220",
  surface: "#111B2E",
  surfaceSecondary: "#0F1829",
  text: "#E6EDF7",
  textSecondary: "#93A4BD",
  border: "#223049",
  primary: "#60A5FA",
  primaryPressed: "#93C5FD",
  success: "#4ADE80",
  danger: "#F87171",
  // Texto escuro sobre o azul claro do tema escuro, para manter o contraste.
  onPrimary: "#0B1220",
  overlay: "rgba(0,0,0,0.64)",
};

export const spacing = {
  xs: 4,
  sm: 8,
  md: 12,
  lg: 16,
  xl: 20,
  xxl: 24,
  xxxl: 32,
} as const;
export const radius = { input: 8, button: 8, card: 8, modal: 12 } as const;
