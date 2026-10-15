import { useEffect } from "react";
import { StyleSheet, Text, View } from "react-native";
import { useRouter } from "expo-router";
import { Screen } from "@/components/common/screen";
import { Brand } from "@/components/common/brand";
import { useAppTheme } from "@/theme/theme-provider";

export default function SplashScreen() {
  const router = useRouter();
  const { colors } = useAppTheme();
  useEffect(() => {
    const timer = setTimeout(() => router.replace("/login"), 1000);
    return () => clearTimeout(timer);
  }, [router]);
  return (
    <Screen scroll={false} contentStyle={styles.screen}>
      <View style={styles.center}>
        <Brand size={38} />
        <Text style={[styles.tagline, { color: colors.textSecondary }]}>
          Gestão de pessoas,{`\n`}na prática.
        </Text>
      </View>
      <Text style={[styles.version, { color: colors.textSecondary }]}>
        v1.0.0
      </Text>
    </Screen>
  );
}
const styles = StyleSheet.create({
  screen: { alignItems: "center", justifyContent: "center" },
  center: { alignItems: "center", gap: 18 },
  tagline: { textAlign: "center", fontSize: 16, lineHeight: 23 },
  version: { position: "absolute", bottom: 24, fontSize: 12 },
});
