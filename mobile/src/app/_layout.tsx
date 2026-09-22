import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Stack, usePathname, useRouter } from "expo-router";
import { StatusBar } from "expo-status-bar";
import { useEffect, useState } from "react";
import { View } from "react-native";
import { AppThemeProvider, useAppTheme } from "@/theme/theme-provider";
import { useSessionStore } from "@/stores/session-store";
import { useNetworkStore } from "@/stores/network-store";
import { BiometricGate } from "@/components/security/biometric-gate";
import { SystemState } from "@/components/common/system-state";
import { getEnvironment } from "@/config/environment";
import { resolveRouteRedirect } from "@/features/auth/services/route-guard";
import { setUnauthorizedHandler } from "@/services/api/http-client";

function Navigation() {
  const { colors, isDark } = useAppTheme();
  const environment = getEnvironment();
  const router = useRouter();
  const pathname = usePathname();
  const { user, hydrated, hydrate, signOut } = useSessionStore();
  const initializeNetwork = useNetworkStore((state) => state.initialize);
  useEffect(() => {
    void hydrate();
  }, [hydrate]);
  useEffect(() => {
    setUnauthorizedHandler(() => void signOut());
    return () => setUnauthorizedHandler(null);
  }, [signOut]);
  useEffect(() => {
    let dispose: (() => void) | undefined;
    void initializeNetwork().then((cleanup) => {
      dispose = cleanup;
    });
    return () => dispose?.();
  }, [initializeNetwork]);
  useEffect(() => {
    if (!hydrated) return;
    const redirect = resolveRouteRedirect(pathname, Boolean(user));
    if (redirect) router.replace(redirect);
  }, [hydrated, pathname, router, user]);
  if (!hydrated)
    return <View style={{ flex: 1, backgroundColor: colors.background }} />;
  if (environment.EXPO_PUBLIC_MAINTENANCE_MODE === "true")
    return <SystemState kind="MAINTENANCE" />;
  return (
    <>
      <StatusBar style={isDark ? "light" : "dark"} />
      <Stack
        screenOptions={{ headerShown: false, animation: "slide_from_right" }}
      >
        <Stack.Screen name="index" options={{ animation: "fade" }} />
        <Stack.Screen name="login" options={{ animation: "fade" }} />
        <Stack.Screen
          name="success"
          options={{ presentation: "transparentModal", animation: "fade" }}
        />
      </Stack>
    </>
  );
}

export default function RootLayout() {
  const [client] = useState(
    () =>
      new QueryClient({
        defaultOptions: { queries: { retry: 1, staleTime: 15_000 } },
      }),
  );
  return (
    <QueryClientProvider client={client}>
      <AppThemeProvider>
        <BiometricGate>
          <Navigation />
        </BiometricGate>
      </AppThemeProvider>
    </QueryClientProvider>
  );
}
