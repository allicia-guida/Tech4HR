import { useState } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { AppInput } from "@/components/common/app-input";
import { PrimaryButton, SecondaryButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import {
  usePendingCorrections,
  useReviewCorrection,
} from "@/features/corrections/hooks/use-corrections";
import { demoEmployee } from "@/features/profile/fixtures/demo-employee";
import { useAppTheme } from "@/theme/theme-provider";

export default function ManagerScreen() {
  const { colors } = useAppTheme();
  const requests = usePendingCorrections();
  const review = useReviewCorrection();
  const [selectedId, setSelectedId] = useState("");
  const [note, setNote] = useState("");
  const selected = requests.data?.find((item) => item.id === selectedId);
  const decide = async (decision: "APPROVED" | "REJECTED") => {
    if (!selected) return;
    await review.mutateAsync({
      id: selected.id,
      decision,
      note: note.trim() || undefined,
    });
    setSelectedId("");
    setNote("");
  };
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Gestor · demonstração" back />
      <View
        style={[styles.employee, { backgroundColor: colors.surfaceSecondary }]}
      >
        <Text style={[styles.employeeName, { color: colors.text }]}>
          {demoEmployee.name}
        </Text>
        <Text style={[styles.employeeInfo, { color: colors.textSecondary }]}>
          {demoEmployee.registration} · {demoEmployee.email}
        </Text>
      </View>
      <Text style={[styles.heading, { color: colors.text }]}>
        Solicitações pendentes
      </Text>
      {!requests.data?.length ? (
        <Text style={[styles.empty, { color: colors.textSecondary }]}>
          Nenhuma solicitação pendente.
        </Text>
      ) : (
        requests.data.map((request) => (
          <Pressable
            key={request.id}
            onPress={() => setSelectedId(request.id)}
            style={[
              styles.card,
              {
                backgroundColor: colors.surface,
                borderColor:
                  selectedId === request.id ? colors.primary : colors.border,
              },
            ]}
          >
            <Text style={[styles.cardTitle, { color: colors.text }]}>
              {request.date} · {request.reason}
            </Text>
            <Text style={[styles.cardText, { color: colors.textSecondary }]}>
              {request.justification}
            </Text>
          </Pressable>
        ))
      )}
      {selected ? (
        <View style={styles.review}>
          <Text style={[styles.heading, { color: colors.text }]}>Decisão</Text>
          <AppInput
            label="Observação"
            value={note}
            onChangeText={setNote}
            multiline
            maxLength={500}
            placeholder="Informe o motivo da decisão"
          />
          <PrimaryButton
            label="Aprovar"
            loading={review.isPending}
            onPress={() => void decide("APPROVED")}
          />
          <SecondaryButton
            label="Rejeitar"
            disabled={review.isPending}
            onPress={() => void decide("REJECTED")}
          />
        </View>
      ) : null}
      <Text
        style={[
          styles.notice,
          {
            color: colors.textSecondary,
            backgroundColor: colors.surfaceSecondary,
          },
        ]}
      >
        Esta área é somente demonstrativa. O backend deverá validar o perfil do
        gestor e registrar a auditoria oficial.
      </Text>
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  employee: { borderRadius: 10, padding: 16, marginTop: 20 },
  employeeName: { fontSize: 16, fontWeight: "800" },
  employeeInfo: { fontSize: 12, marginTop: 5 },
  heading: { fontSize: 18, fontWeight: "800", marginTop: 24, marginBottom: 12 },
  empty: { fontSize: 14, paddingVertical: 18 },
  card: { borderWidth: 1, borderRadius: 9, padding: 14, marginBottom: 10 },
  cardTitle: { fontSize: 14, fontWeight: "700" },
  cardText: { fontSize: 13, lineHeight: 19, marginTop: 6 },
  review: { gap: 12 },
  notice: {
    marginTop: 26,
    padding: 14,
    borderRadius: 8,
    fontSize: 13,
    lineHeight: 20,
  },
});
