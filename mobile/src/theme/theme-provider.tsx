import {
  DarkTheme,
  DefaultTheme,
  ThemeProvider as NavigationThemeProvider,
} from "expo-router";
import {
  createContext,
  PropsWithChildren,
  useContext,
  useEffect,
  useMemo,
} from "react";
import { ColorSchemeName, useColorScheme } from "react-native";
import { darkColors, lightColors, ThemeColors } from "./tokens";
import { useThemeStore } from "@/stores/theme-store";

type AppTheme = {
  colors: ThemeColors;
  isDark: boolean;
  colorScheme: NonNullable<ColorSchemeName>;
};
const AppThemeContext = createContext<AppTheme>({
  colors: lightColors,
  isDark: false,
  colorScheme: "light",
});

export function AppThemeProvider({ children }: PropsWithChildren) {
  const systemScheme = useColorScheme() ?? "light";
  const { preference, hydrate } = useThemeStore();
  useEffect(() => {
    void hydrate();
  }, [hydrate]);
  const colorScheme = preference === "system" ? systemScheme : preference;
  const value = useMemo(
    () => ({
      colors: colorScheme === "dark" ? darkColors : lightColors,
      isDark: colorScheme === "dark",
      colorScheme,
    }),
    [colorScheme],
  );
  const navigationTheme = value.isDark
    ? {
        ...DarkTheme,
        colors: {
          ...DarkTheme.colors,
          background: value.colors.background,
          card: value.colors.surface,
          border: value.colors.border,
          primary: value.colors.primary,
          text: value.colors.text,
        },
      }
    : {
        ...DefaultTheme,
        colors: {
          ...DefaultTheme.colors,
          background: value.colors.background,
          card: value.colors.surface,
          border: value.colors.border,
          primary: value.colors.primary,
          text: value.colors.text,
        },
      };
  return (
    <AppThemeContext.Provider value={value}>
      <NavigationThemeProvider value={navigationTheme}>
        {children}
      </NavigationThemeProvider>
    </AppThemeContext.Provider>
  );
}

export const useAppTheme = () => useContext(AppThemeContext);
