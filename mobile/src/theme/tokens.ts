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
  background: "#F5F6F8",
  surface: "#FFFFFF",
  surfaceSecondary: "#EEF0F4",
  text: "#14181F",
  textSecondary: "#566070",
  border: "#D9DDE3",
  primary: "#1D4E89",
  primaryPressed: "#163F70",
  success: "#1B6B3A",
  danger: "#B3261E",
  onPrimary: "#FFFFFF",
  overlay: "rgba(20,24,31,0.5)",
};

export const darkColors: ThemeColors = {
  background: "#0F1318",
  surface: "#171C23",
  surfaceSecondary: "#1F262F",
  text: "#E7EAEE",
  textSecondary: "#9AA4B2",
  border: "#2B333E",
  primary: "#7FB0E6",
  primaryPressed: "#A0C4EE",
  success: "#6CC08A",
  danger: "#F0857D",
  // Texto escuro sobre o azul claro do tema escuro, para manter o contraste.
  onPrimary: "#0F1318",
  overlay: "rgba(0,0,0,0.6)",
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
export const radius = { input: 4, button: 4, card: 6, modal: 6 } as const;
