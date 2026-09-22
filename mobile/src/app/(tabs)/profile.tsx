import * as ImagePicker from "expo-image-picker";
import { Image, Pressable, StyleSheet, Text, View } from "react-native";
import { useRouter } from "expo-router";
import { useQueryClient } from "@tanstack/react-query";
import { AppHeader } from "@/components/common/app-header";
import { Screen } from "@/components/common/screen";
import { MenuRow } from "@/components/profile/menu-row";
import { useAppTheme } from "@/theme/theme-provider";
import { useSessionStore } from "@/stores/session-store";

export default function ProfileScreen() {
  const { colors } = useAppTheme();
  const router = useRouter();
  const queryClient = useQueryClient();
  const { user, signOut, updateProfile } = useSessionStore();
  if (!user) return null;
  const employee = user;
  const logout = async () => {
    await signOut();
    queryClient.clear();
    router.replace("/login");
  };
  const changePhoto = async () => {
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ["images"],
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.75,
    });
    if (!result.canceled)
      await updateProfile({ photoUri: result.assets[0].uri });
  };
  return (
    <Screen contentStyle={styles.content}>
      <AppHeader title="Perfil" />
      <View style={styles.identity}>
        <Pressable
          onPress={() => void changePhoto()}
          accessibilityLabel="Alterar foto de perfil"
          style={[styles.avatar, { backgroundColor: colors.surfaceSecondary }]}
        >
          {employee.photoUri ? (
            <Image source={{ uri: employee.photoUri }} style={styles.photo} />
          ) : (
            <Text style={[styles.initials, { color: colors.text }]}>
              {employee.initials}
            </Text>
          )}
        </Pressable>
        <Text style={[styles.name, { color: colors.text }]}>
          {employee.name}
        </Text>
        <Text style={[styles.email, { color: colors.textSecondary }]}>
          {employee.email}
        </Text>
      </View>
      <View>
        <MenuRow
          label="Dados pessoais"
          onPress={() => router.push("/personal-data")}
        />
        <MenuRow
          label="Minha jornada"
          onPress={() => router.push("/work-rules")}
        />
        <MenuRow
          label="Correções de ponto"
          onPress={() => router.push("/corrections")}
        />
        <MenuRow
          label="Férias e ausências"
          onPress={() => router.push("/absences")}
        />
        <MenuRow
          label="Configurações"
          onPress={() => router.push("/settings")}
        />
        <MenuRow
          label="Privacidade e termos"
          onPress={() => router.push("/privacy")}
        />
        <MenuRow label="Sobre o app" onPress={() => router.push("/about")} />
        <MenuRow label="Sair" onPress={() => void logout()} />
      </View>
    </Screen>
  );
}
const styles = StyleSheet.create({
  content: { paddingBottom: 24 },
  identity: { alignItems: "center", paddingVertical: 32 },
  avatar: {
    width: 76,
    height: 76,
    borderRadius: 38,
    alignItems: "center",
    justifyContent: "center",
  },
  initials: { fontSize: 22, fontWeight: "700" },
  photo: { width: 76, height: 76, borderRadius: 38 },
  name: { fontSize: 18, fontWeight: "700", marginTop: 16 },
  email: { fontSize: 14, marginTop: 6 },
});
