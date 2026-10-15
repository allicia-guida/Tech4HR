// Texto puro curto (como "Marcação fora de sequência...") é a mensagem da API
// e pode ir para a tela. Página HTML ou texto longo (erro de gateway, por
// exemplo) não vai.
const plainTextMessage = (raw: string) => {
  const text = raw.trim();
  return text && text.length <= 300 && !text.startsWith("<") ? text : null;
};

// Extrai a mensagem de uma resposta de erro da API. Aceita os formatos que
// as APIs do Tech4Hr devolvem: { message }, { title } do ASP.NET, { errors }
// de validação e texto puro.
export const parseErrorMessage = (raw: string, fallback: string) => {
  if (!raw) return fallback;
  try {
    const body = JSON.parse(raw) as
      | string
      | {
          message?: unknown;
          title?: unknown;
          errors?: Record<string, unknown>;
        };
    if (typeof body === "string") return plainTextMessage(body) ?? fallback;
    if (typeof body.message === "string") return body.message;
    if (typeof body.title === "string") return body.title;
    const validation = Object.values(body.errors ?? {}).flat();
    const first = validation.find((item) => typeof item === "string");
    return typeof first === "string" ? first : fallback;
  } catch {
    return plainTextMessage(raw) ?? fallback;
  }
};
