import {
  AlertTriangle,
  CloudOff,
  LogOut,
  RefreshCw,
} from "lucide-react-native";
import { ReactNode } from "react";
import { StyleSheet, Text, View } from "react-native";
import { SecondaryButton } from "./buttons";
import { useAppTheme } from "@/theme/theme-provider";

export type SystemStateKind =
  | "OFFLINE"
  | "SESSION_EXPIRED"
  | "SERVER_ERROR"
  | "MAINTENANCE"
  | "UPDATE_REQUIRED";

const copy: Record<SystemStateKind, { title: string; message: string }> = {
  OFFLINE: {
    title: "Sem conexão",
    message:
      "Verifique sua internet. Registros não serão confirmados enquanto estiver offline.",
  },
  SESSION_EXPIRED: {
    title: "Sessão encerrada",
    message: "Entre novamente para proteger seus dados.",
  },
  SERVER_ERROR: {
    title: "Serviço indisponível",
    message:
      "Não foi possível acessar o servidor. Tente novamente em instantes.",
  },
  MAINTENANCE: {
    title: "Em manutenção",
    message: "Estamos preparando o serviço. Tente novamente mais tarde.",
  },
  UPDATE_REQUIRED: {
    title: "Atualização necessária",
    message:
      "Instale a versão mais recente para continuar usando o aplicativo.",
  },
};

export function SystemState({
  kind,
  action,
}: {
  kind: SystemStateKind;
  action?: () => void;
}) {
  const { colors } = useAppTheme();
  const icons: Record<SystemStateKind, ReactNode> = {
    OFFLINE: <CloudOff color={colors.danger} size={36} />,
    SESSION_EXPIRED: <LogOut color={colors.danger} size={36} />,
    SERVER_ERROR: <AlertTriangle color={colors.danger} size={36} />,
    MAINTENANCE: <RefreshCw color={colors.primary} size={36} />,
    UPDATE_REQUIRED: <RefreshCw color={colors.primary} size={36} />,
  };
  return (
    <View style={styles.state} accessibilityRole="alert">
      {icons[kind]}
      <Text style={[styles.title, { color: colors.text }]}>
        {copy[kind].title}
      </Text>
      <Text style={[styles.message, { color: colors.textSecondary }]}>
        {copy[kind].message}
      </Text>
      {action ? (
        <SecondaryButton label="Tentar novamente" onPress={action} />
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  state: {
    minHeight: 260,
    alignItems: "center",
    justifyContent: "center",
    padding: 24,
    gap: 12,
  },
  title: { fontSize: 20, fontWeight: "800", textAlign: "center" },
  message: { fontSize: 14, lineHeight: 21, textAlign: "center" },
});
