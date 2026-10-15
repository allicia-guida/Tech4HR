import { Employee } from "@/features/profile/types/employee";

// Resposta de POST /api/auth/login-funcionario da API do Tech4Hr.
export type Tech4hrLoginResponse = {
  token: string;
  tipo: string;
  expiraEm: string;
  funcionario: {
    idFuncionario: number;
    nome: string;
    sobrenome: string;
    emailCorporativo: string;
    ativo: boolean;
    nivelAcesso?: "FUNCIONARIO" | "OPERACIONAL";
    dataAdmissao?: string;
  };
};

export type Tech4hrSignIn = {
  user: Employee;
  accessToken: string;
  expiresAt: string;
};

const initialsOf = (name: string) =>
  name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part.charAt(0))
    .join("")
    .toUpperCase();

export const toTech4hrSignIn = (response: Tech4hrLoginResponse): Tech4hrSignIn => {
  const person = response.funcionario;
  const name = `${person.nome} ${person.sobrenome}`.trim();
  return {
    user: {
      id: String(person.idFuncionario),
      name,
      email: person.emailCorporativo,
      registration: `FUNC-${String(person.idFuncionario).padStart(4, "0")}`,
      initials: initialsOf(name),
      phone: "",
      department: "",
      jobTitle:
        person.nivelAcesso === "OPERACIONAL" ? "Operacional" : "Funcionário",
      hireDate: person.dataAdmissao ? person.dataAdmissao.slice(0, 10) : "",
      workPolicyName: "Jornada padrão",
    },
    accessToken: response.token,
    expiresAt: response.expiraEm,
  };
};
