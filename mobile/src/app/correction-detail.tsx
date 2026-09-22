import { useLocalSearchParams, useRouter } from "expo-router";
import { StyleSheet, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { SecondaryButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import { LoadingState } from "@/components/common/states";
import {
  useCancelCorrection,
  useCorrection,
} from "@/features/corrections/hooks/use-corrections";
import { demoEmployee } from "@/features/profile/fixtures/demo-employee";
import { useSessionStore } from "@/stores/session-store";
import { useAppTheme } from "@/theme/theme-provider";

export default function CorrectionDetailScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const { id = "" } = useLocalSearchParams<{ id?: string }>();
  const employee = useSessionStore((state) => state.user) ?? demoEmployee;
  const request = useCorrection(id);
  const cancel = useCancelCorrection(employee.id);
  if (request.isLoading)
    return (
      <Screen>
        <AppHeader title="Solicitação" back />
        <LoadingState />
      </Screen>
    );
  if (!request.data)
    return (
      <Screen>
        <AppHeader title="Solicitação" back />
        <Text style={{ color: colors.text }}>Solicitação não encontrada.</Text>
      </Screen>
    );
  const item = request.data;
  const cancelRequest = async () => {
    await cancel.mutateAsync(item.id);
    await request.refetch();
  };
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Detalhe da correção" back />
      <View
        style={[
          styles.card,
          { backgroundColor: colors.surface, borderColor: colors.border },
        ]}
      >
        <Field label="Data" value={item.date} />
        <Field label="Status" value={item.status} />
        <Field label="Motivo" value={item.reason} />
        <Field label="Justificativa" value={item.justification} />
        {item.proposedEntries.map((entry, index) => (
          <Field
            key={`${entry.timestamp}-${index}`}
            label="Horário proposto"
            value={`${entry.type} · ${new Date(entry.timestamp).toLocaleString("pt-BR")}`}
          />
        ))}
        <Field
          label="Anexos"
          value={
            item.attachments.length
              ? item.attachments.map((file) => file.name).join(", ")
              : "Nenhum"
          }
        />
        {item.reviewerNote ? (
          <Field label="Observação do gestor" value={item.reviewerNote} />
        ) : null}
      </View>
      <Text style={[styles.heading, { color: colors.text }]}>
        Histórico da aprovação
      </Text>
      {item.history.map((event) => (
        <View
          key={event.id}
          style={[styles.event, { borderLeftColor: colors.primary }]}
        >
          <Text style={[styles.eventAction, { color: colors.text }]}>
            {event.action}
          </Text>
          <Text style={[styles.eventDate, { color: colors.textSecondary }]}>
            {new Date(event.at).toLocaleString("pt-BR")}
          </Text>
          {event.note ? (
            <Text style={[styles.eventNote, { color: colors.textSecondary }]}>
              {event.note}
            </Text>
          ) : null}
        </View>
      ))}
      {item.status === "PENDING" ? (
        <SecondaryButton
          label="Cancelar solicitação"
          loading={cancel.isPending}
          onPress={() => void cancelRequest()}
        />
      ) : null}
      <SecondaryButton label="Voltar" onPress={() => router.back()} />
    </Screen>
  );
}

function Field({ label, value }: { label: string; value: string }) {
  const { colors } = useAppTheme();
  return (
    <View style={styles.field}>
      <Text style={[styles.fieldLabel, { color: colors.textSecondary }]}>
        {label}
      </Text>
      <Text style={[styles.fieldValue, { color: colors.text }]}>{value}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32, gap: 14 },
  card: {
    borderWidth: 1,
    borderRadius: 10,
    padding: 16,
    gap: 16,
    marginTop: 20,
  },
  field: { gap: 4 },
  fieldLabel: { fontSize: 12 },
  fieldValue: { fontSize: 14, lineHeight: 20, fontWeight: "600" },
  heading: { fontSize: 17, fontWeight: "800", marginTop: 8 },
  event: { borderLeftWidth: 3, paddingLeft: 12, paddingVertical: 6 },
  eventAction: { fontSize: 14, fontWeight: "700" },
  eventDate: { fontSize: 12, marginTop: 3 },
  eventNote: { fontSize: 13, marginTop: 5 },
});
