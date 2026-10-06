import { describe, expect, it } from "vitest";
import { parseErrorMessage } from "./error-message";

const FALLBACK = "Não foi possível concluir a solicitação.";

describe("mensagem de erro da API", () => {
  it("lê o texto puro que o Conflict da API devolve", () => {
    expect(
      parseErrorMessage(
        "Marcação fora de sequência. Registre a próxima etapa da jornada.",
        FALLBACK,
      ),
    ).toBe("Marcação fora de sequência. Registre a próxima etapa da jornada.");
  });

  it("lê o campo message do JSON", () => {
    expect(
      parseErrorMessage('{"message":"E-mail ou senha inválidos."}', FALLBACK),
    ).toBe("E-mail ou senha inválidos.");
  });

  it("lê o title de um erro do ASP.NET", () => {
    expect(
      parseErrorMessage('{"title":"One or more validation errors occurred."}', FALLBACK),
    ).toBe("One or more validation errors occurred.");
  });

  it("lê o primeiro erro de validação", () => {
    expect(
      parseErrorMessage('{"errors":{"Senha":["A senha é obrigatória."]}}', FALLBACK),
    ).toBe("A senha é obrigatória.");
  });

  it("lê texto puro vindo como string JSON", () => {
    expect(parseErrorMessage('"Jornada já registrada."', FALLBACK)).toBe(
      "Jornada já registrada.",
    );
  });

  it("não mostra uma página HTML de erro de gateway", () => {
    expect(
      parseErrorMessage("<html><body>502 Bad Gateway</body></html>", FALLBACK),
    ).toBe(FALLBACK);
  });

  it("não mostra um texto longo demais", () => {
    expect(parseErrorMessage("x".repeat(400), FALLBACK)).toBe(FALLBACK);
  });

  it("usa a mensagem padrão quando a resposta vem vazia", () => {
    expect(parseErrorMessage("", FALLBACK)).toBe(FALLBACK);
  });
});
