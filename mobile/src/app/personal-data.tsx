import { useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { AppHeader } from "@/components/common/app-header";
import { AppInput } from "@/components/common/app-input";
import { PrimaryButton } from "@/components/common/buttons";
import { Screen } from "@/components/common/screen";
import { demoEmployee } from "@/features/profile/fixtures/demo-employee";
import { useSessionStore } from "@/stores/session-store";
import { useAppTheme } from "@/theme/theme-provider";

export default function PersonalDataScreen() {
  const { colors } = useAppTheme();
  const { user, updateProfile } = useSessionStore();
  const employee = user ?? demoEmployee;
  const [phone, setPhone] = useState(employee.phone);
  const [saved, setSaved] = useState(false);
  const save = async () => {
    await updateProfile({ phone: phone.trim().slice(0, 24) });
    setSaved(true);
  };
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Dados pessoais" back />
      <Text style={[styles.heading, { color: colors.text }]}>
        Informações pessoais
      </Text>
      <View style={styles.fields}>
        <Data label="Nome" value={employee.name} />
        <Data label="E-mail" value={employee.email} />
        <AppInput
          label="Telefone"
          value={phone}
          onChangeText={(value) => {
            setPhone(value);
            setSaved(false);
          }}
          keyboardType="phone-pad"
          maxLength={24}
        />
        <PrimaryButton label="Salvar telefone" onPress={() => void save()} />
        {saved ? (
          <Text style={[styles.saved, { color: colors.success }]}>
            Telefone salvo localmente.
          </Text>
        ) : null}
      </View>
      <Text style={[styles.heading, { color: colors.text }]}>
        Dados profissionais
      </Text>
      <View
        style={[
          styles.card,
          { backgroundColor: colors.surface, borderColor: colors.border },
        ]}
      >
        <Data label="Matrícula" value={employee.registration} />
        <Data label="Cargo" value={employee.jobTitle} />
        <Data label="Departamento" value={employee.department} />
        <Data label="Admissão" value={employee.hireDate} />
        <Data label="Jornada contratual" value={employee.workPolicyName} />
      </View>
    </Screen>
  );
}

function Data({ label, value }: { label: string; value: string }) {
  const { colors } = useAppTheme();
  return (
    <View style={styles.data}>
      <Text style={[styles.label, { color: colors.textSecondary }]}>
        {label}
      </Text>
      <Text style={[styles.value, { color: colors.text }]}>{value}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: 32 },
  heading: { fontSize: 18, fontWeight: "800", marginTop: 24, marginBottom: 14 },
  fields: { gap: 16 },
  card: { borderWidth: 1, borderRadius: 10, padding: 16, gap: 16 },
  data: { gap: 4 },
  label: { fontSize: 12 },
  value: { fontSize: 15, fontWeight: "600" },
  saved: { fontSize: 13, textAlign: "center", fontWeight: "600" },
});
