import { LoginForm } from "../schemas/login-schema";
import { Employee } from "@/features/profile/types/employee";
import { getEnvironment } from "@/config/environment";
import { ApiError, apiRequest } from "@/services/api/http-client";

type LoginResponse = {
  token: string;
  tokenType: "Bearer";
  expiresAtUtc: string;
  user: {
    id: string;
    name: string;
    email: string;
    role: "ADMIN" | "GESTOR" | "FUNCIONARIO";
  };
};

export type SignInResult = {
  user: Employee;
  accessToken?: string;
  expiresAt?: string;
};

const initials = (name: string) =>
  name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part.charAt(0))
    .join("")
    .toUpperCase();

export const authService = {
  async signIn(credentials: LoginForm): Promise<SignInResult> {
    const environment = getEnvironment();
    if (!environment.EXPO_PUBLIC_API_URL) {
      throw new ApiError(0, "A API segura do Tech4HR não está configurada.");
    }
    const response = await apiRequest<LoginResponse>(
      "/api/v1/auth/login",
      {
        method: "POST",
        body: JSON.stringify({
          email: credentials.email.trim().toLowerCase(),
          password: credentials.password,
        }),
      },
    );
    const employee = response.user;
    return {
      user: {
        id: employee.id,
        name: employee.name,
        email: employee.email,
        registration: `FUNC-${employee.id.slice(0, 8).toUpperCase()}`,
        initials: initials(employee.name),
        phone: "",
        department: "",
        jobTitle:
          employee.role === "ADMIN"
            ? "Administrador"
            : employee.role === "GESTOR"
              ? "Gestor"
              : "Funcionário",
        hireDate: "",
        workPolicyName: "Jornada padrão",
      },
      accessToken: response.token,
      expiresAt: response.expiresAtUtc,
    };
  },
  async requestPasswordReset(
    email: string,
  ): Promise<{ delivery: "email" }> {
    if (!getEnvironment().EXPO_PUBLIC_API_URL) {
      throw new ApiError(0, "A API segura do Tech4HR não está configurada.");
    }
    await apiRequest<{ message: string }>("/api/v1/auth/forgot-password", {
      method: "POST",
      body: JSON.stringify({ email: email.trim().toLowerCase() }),
    });
    return { delivery: "email" };
  },
};
