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

export const lightColors: ThemeColors = {
  background: "#F8F9FB",
  surface: "#FFFFFF",
  surfaceSecondary: "#F1F5F9",
  text: "#0F172A",
  textSecondary: "#64748B",
  border: "#E2E8F0",
  primary: "#146EF5",
  primaryPressed: "#0F5DCE",
  success: "#22C55E",
  danger: "#EF4444",
  onPrimary: "#FFFFFF",
  overlay: "rgba(15,23,42,0.48)",
};

export const darkColors: ThemeColors = {
  background: "#111820",
  surface: "#18212B",
  surfaceSecondary: "#202B36",
  text: "#F8FAFC",
  textSecondary: "#94A3B8",
  border: "#2B3744",
  primary: "#1473E6",
  primaryPressed: "#0F63C8",
  success: "#22C55E",
  danger: "#EF4444",
  onPrimary: "#FFFFFF",
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
