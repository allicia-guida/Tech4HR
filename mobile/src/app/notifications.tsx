import { useEffect, useState } from "react";
import { StyleSheet, Switch, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { useNotificationStore } from "@/stores/notification-store";
import { useAppTheme } from "@/theme/theme-provider";

const reminderLabels = [
  "Entrada às 08:00",
  "Retorno do intervalo às 13:00",
  "Saída às 17:00",
  "Jornada incompleta às 18:30",
];

export default function NotificationsScreen() {
  const { colors } = useAppTheme();
  const { enabled, hydrated, hydrate, setEnabled } = useNotificationStore();
  const [message, setMessage] = useState("");
  useEffect(() => {
    void hydrate();
  }, [hydrate]);
  const toggle = async (value: boolean) => {
    try {
      await setEnabled(value);
      setMessage(
        value
          ? "Lembretes locais programados."
          : "Lembretes locais desativados.",
      );
    } catch {
      setMessage("Permita notificações nas configurações do aparelho.");
    }
  };
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Notificações" back />
      <View style={[styles.row, { borderBottomColor: colors.border }]}>
        <View style={styles.copy}>
          <Text style={[styles.title, { color: colors.text }]}>
            Lembretes da jornada
          </Text>
          <Text style={[styles.description, { color: colors.textSecondary }]}>
            Programados localmente de segunda a sexta.
          </Text>
        </View>
        <Switch
          disabled={!hydrated}
          value={enabled}
          onValueChange={(value) => void toggle(value)}
        />
      </View>
      <View style={styles.list}>
        {reminderLabels.map((label) => (
          <Text
            key={label}
            style={[styles.item, { color: colors.textSecondary }]}
          >
            • {label}
          </Text>
        ))}
      </View>
      {message ? (
        <Text style={[styles.message, { color: colors.textSecondary }]}>
          {message}
        </Text>
      ) : null}
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  row: {
    minHeight: 88,
    flexDirection: "row",
    alignItems: "center",
    borderBottomWidth: StyleSheet.hairlineWidth,
    marginTop: 16,
  },
  copy: { flex: 1, paddingRight: 16 },
  title: { fontSize: 16, fontWeight: "700" },
  description: { fontSize: 13, lineHeight: 19, marginTop: 5 },
  list: { gap: 10, marginTop: 22 },
  item: { fontSize: 14 },
  message: { fontSize: 13, marginTop: 22 },
});
