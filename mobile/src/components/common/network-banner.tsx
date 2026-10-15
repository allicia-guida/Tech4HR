import { WifiOff } from "lucide-react-native";
import { StyleSheet, Text, View } from "react-native";
import { useNetworkStore } from "@/stores/network-store";
import { useAppTheme } from "@/theme/theme-provider";

export function NetworkBanner() {
  const { colors } = useAppTheme();
  const offline = useNetworkStore(
    (state) => state.initialized && (!state.connected || !state.reachable),
  );
  if (!offline) return null;
  return (
    <View style={[styles.banner, { backgroundColor: colors.danger }]}>
      <WifiOff size={16} color={colors.onPrimary} />
      <Text style={[styles.text, { color: colors.onPrimary }]}>
        Sem internet · registros não serão confirmados
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  banner: {
    minHeight: 36,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 8,
    paddingHorizontal: 12,
  },
  text: { fontSize: 12, fontWeight: "700", textAlign: "center" },
});
