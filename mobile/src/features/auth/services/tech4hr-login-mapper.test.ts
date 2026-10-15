import { describe, expect, it } from "vitest";
import { toTech4hrSignIn, Tech4hrLoginResponse } from "./tech4hr-login-mapper";

const response = (
  overrides: Partial<Tech4hrLoginResponse["funcionario"]> = {},
): Tech4hrLoginResponse => ({
  token: "jwt-de-teste",
  tipo: "Bearer",
  expiraEm: "2026-10-06T01:55:09.123Z",
  funcionario: {
    idFuncionario: 12,
    nome: "Rita",
    sobrenome: "Operacional",
    emailCorporativo: "rita@empresa.com",
    ativo: true,
    nivelAcesso: "FUNCIONARIO",
    dataAdmissao: "2025-02-10T00:00:00",
    ...overrides,
  },
});

describe("login pela API do Tech4Hr", () => {
  it("monta o perfil do funcionário", () => {
    const result = toTech4hrSignIn(response());

    expect(result.user).toMatchObject({
      id: "12",
      name: "Rita Operacional",
      email: "rita@empresa.com",
      registration: "FUNC-0012",
      initials: "RO",
      jobTitle: "Funcionário",
      hireDate: "2025-02-10",
    });
  });

  it("guarda o token e a validade para a sessão", () => {
    const result = toTech4hrSignIn(response());

    expect(result.accessToken).toBe("jwt-de-teste");
    expect(result.expiresAt).toBe("2026-10-06T01:55:09.123Z");
  });

  it("identifica o operacional pelo nível de acesso", () => {
    const result = toTech4hrSignIn(response({ nivelAcesso: "OPERACIONAL" }));

    expect(result.user.jobTitle).toBe("Operacional");
  });

  it("aceita uma API que ainda não manda o nível nem a admissão", () => {
    const result = toTech4hrSignIn(
      response({ nivelAcesso: undefined, dataAdmissao: undefined }),
    );

    expect(result.user.jobTitle).toBe("Funcionário");
    expect(result.user.hireDate).toBe("");
  });
});
