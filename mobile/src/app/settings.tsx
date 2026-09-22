import { StyleSheet, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { MenuRow } from "@/components/profile/menu-row";
import { ThemeSelector } from "@/components/profile/theme-selector";
import { useAppTheme } from "@/theme/theme-provider";
import { useRouter } from "expo-router";

export default function SettingsScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader back title="Configurações" />
      <Text style={[styles.section, { color: colors.textSecondary }]}>
        TEMA
      </Text>
      <ThemeSelector />
      <Text style={[styles.section, { color: colors.textSecondary }]}>
        PREFERÊNCIAS
      </Text>
      <View>
        <MenuRow
          label="Notificações"
          onPress={() => router.push("/notifications")}
        />
        <MenuRow label="Segurança" onPress={() => router.push("/security")} />
        <MenuRow label="Privacidade" onPress={() => router.push("/privacy")} />
        <MenuRow
          label="Operações pendentes"
          onPress={() => router.push("/pending-operations")}
        />
        <MenuRow label="Sobre o app" onPress={() => router.push("/about")} />
      </View>
    </Screen>
  );
}
const styles = StyleSheet.create({
  content: { paddingBottom: 24 },
  section: {
    fontSize: 12,
    fontWeight: "700",
    letterSpacing: 0.6,
    marginTop: 28,
    marginBottom: 8,
  },
});
