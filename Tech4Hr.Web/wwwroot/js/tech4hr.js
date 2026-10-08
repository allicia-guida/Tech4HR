/* ==========================================================================
   TECH4HR, COMPORTAMENTOS DA INTERFACE
   Sem dependências. Cada bloco cuida de uma coisa e só age se a tela
   tiver os elementos de que precisa.

   1. Modo de contraste para daltonismo
   2. Mostrar e ocultar senha
   3. Abas do login (funcionário e administrador)
   4. Busca e filtro da tabela de funcionários
   5. Diálogo de confirmação dos formulários com data-confirm
   6. Proteção contra clique duplo nos formulários de envio
   7. Avisos de sucesso que somem sozinhos
   ========================================================================== */

(function () {
    "use strict";

    var CHAVE_CONTRASTE = "tech4hr-contraste";

    function lerPreferencia(chave) {
        try {
            return window.localStorage.getItem(chave);
        } catch (erro) {
            return null;
        }
    }

    function gravarPreferencia(chave, valor) {
        try {
            window.localStorage.setItem(chave, valor);
        } catch (erro) {
            /* Navegação privada ou armazenamento bloqueado: a escolha vale só nesta página. */
        }
    }


    /* ---------- 1. Modo de contraste para daltonismo ---------- */

    (function modoContraste() {
        var raiz = document.documentElement;
        var botao = document.getElementById("toggle-acessibilidade");

        if (!botao) {
            return;
        }

        function aplicar(ativo, salvar) {
            raiz.classList.toggle("accessibility-daltonic", ativo);

            botao.setAttribute("aria-pressed", String(ativo));

            var texto = ativo
                ? "Desativar modo de contraste para daltonismo"
                : "Ativar modo de contraste para daltonismo";

            botao.setAttribute("aria-label", texto);
            botao.title = texto;

            if (salvar) {
                gravarPreferencia(CHAVE_CONTRASTE, String(ativo));
            }
        }

        aplicar(raiz.classList.contains("accessibility-daltonic"), false);

        botao.addEventListener("click", function () {
            aplicar(!raiz.classList.contains("accessibility-daltonic"), true);
        });
    })();


    /* ---------- 2. Mostrar e ocultar senha ---------- */

    document.addEventListener("click", function (evento) {
        var botao = evento.target.closest("[data-toggle-password]");

        if (!botao) {
            return;
        }

        var campo = botao.parentElement.querySelector("input");

        if (!campo) {
            return;
        }

        var mostrar = campo.type === "password";

        campo.type = mostrar ? "text" : "password";
        botao.setAttribute("aria-label", mostrar ? "Ocultar senha" : "Mostrar senha");

        var uso = botao.querySelector("use");

        if (uso) {
            uso.setAttribute("href", mostrar ? "#i-eye-off" : "#i-eye");
        }
    });


    /* ---------- 3. Abas do login ---------- */

    (function abasDoLogin() {
        var formulario = document.getElementById("loginForm");

        if (!formulario) {
            return;
        }

        var opcoes = formulario.querySelectorAll(".login-option");
        var enviar = document.getElementById("loginSubmitButton");
        var dica = document.getElementById("loginHint");

        var fluxos = {
            funcionario: {
                acao: formulario.dataset.actionFuncionario,
                botao: "Entrar como funcionário",
                dica: "Funcionários e operacionais entram por aqui."
            },
            administrativo: {
                acao: formulario.dataset.actionAdministrativo,
                botao: "Entrar como administrador",
                dica: "Acesso exclusivo de administradores."
            }
        };

        function escolher(nome) {
            var fluxo = fluxos[nome];

            formulario.setAttribute("action", fluxo.acao);
            enviar.textContent = fluxo.botao;
            dica.textContent = fluxo.dica;

            opcoes.forEach(function (opcao) {
                var ativa = opcao.dataset.flow === nome;

                opcao.classList.toggle("active", ativa);
                opcao.setAttribute("aria-pressed", String(ativa));
            });
        }

        opcoes.forEach(function (opcao) {
            opcao.addEventListener("click", function () {
                escolher(opcao.dataset.flow);
            });
        });
    })();


    /* ---------- 4. Busca e filtro da tabela de funcionários ---------- */

    (function filtroDeFuncionarios() {
        var busca = document.getElementById("buscaFuncionario");
        var filtroStatus = document.getElementById("filtroStatusFuncionario");
        var tabela = document.getElementById("tabelaFuncionarios");

        if (!tabela) {
            return;
        }

        function aplicar() {
            var termo = (busca ? busca.value : "").trim().toLowerCase();
            var status = filtroStatus ? filtroStatus.value : "todos";

            tabela.querySelectorAll("tbody tr").forEach(function (linha) {
                var texto = linha.textContent.toLowerCase();
                var statusDaLinha = (linha.dataset.status || "").toLowerCase();

                var passaNaBusca = !termo || texto.indexOf(termo) !== -1;

                var passaNoStatus =
                    status === "todos" ||
                    (status === "ativos" && statusDaLinha === "ativo") ||
                    (status === "inativos" && statusDaLinha === "inativo");

                linha.hidden = !(passaNaBusca && passaNoStatus);
            });
        }

        if (busca) {
            busca.addEventListener("input", aplicar);
        }

        if (filtroStatus) {
            filtroStatus.addEventListener("change", aplicar);
        }
    })();


    /* ---------- 5. Diálogo de confirmação ---------- */

    // Formulários com data-confirm pedem confirmação antes de enviar.
    // Atributos aceitos: data-confirm (mensagem), data-confirm-title,
    // data-confirm-ok (texto do botão) e data-confirm-danger="true" (botão vermelho).

    var dialogo = null;

    function obterDialogo() {
        if (dialogo) {
            return dialogo;
        }

        dialogo = document.createElement("dialog");
        dialogo.className = "confirm-dialog";
        dialogo.setAttribute("aria-labelledby", "confirmTitulo");
        dialogo.setAttribute("aria-describedby", "confirmMensagem");

        dialogo.innerHTML =
            '<form method="dialog">' +
            '  <div class="confirm-dialog-body">' +
            '    <h3 id="confirmTitulo"></h3>' +
            '    <p id="confirmMensagem"></p>' +
            "  </div>" +
            '  <div class="confirm-dialog-actions">' +
            '    <button type="submit" value="cancelar" class="btn-secondary" autofocus>Cancelar</button>' +
            '    <button type="submit" value="confirmar" class="btn-primary" id="confirmOk">Confirmar</button>' +
            "  </div>" +
            "</form>";

        document.body.appendChild(dialogo);
        return dialogo;
    }

    function perguntar(dados) {
        var titulo = dados.confirmTitle || "Confirmar ação";
        var mensagem = dados.confirm;

        // Navegador sem <dialog>: usa a janela padrão.
        if (typeof HTMLDialogElement !== "function") {
            return Promise.resolve(window.confirm(titulo + "\n\n" + mensagem));
        }

        var caixa = obterDialogo();
        var botaoOk = caixa.querySelector("#confirmOk");

        caixa.querySelector("#confirmTitulo").textContent = titulo;
        caixa.querySelector("#confirmMensagem").textContent = mensagem;

        botaoOk.textContent = dados.confirmOk || "Confirmar";
        botaoOk.classList.toggle("btn-danger", dados.confirmDanger === "true");
        botaoOk.classList.toggle("btn-primary", dados.confirmDanger !== "true");

        return new Promise(function (resolver) {
            function aoFechar() {
                caixa.removeEventListener("close", aoFechar);
                resolver(caixa.returnValue === "confirmar");
            }

            caixa.returnValue = "";
            caixa.addEventListener("close", aoFechar);
            caixa.showModal();
        });
    }

    document.addEventListener("submit", function (evento) {
        var formulario = evento.target;

        if (!(formulario instanceof HTMLFormElement) ||
            !formulario.dataset.confirm ||
            formulario.dataset.confirmado === "true") {
            return;
        }

        evento.preventDefault();
        evento.stopImmediatePropagation();

        var enviadoPor = evento.submitter;

        perguntar(formulario.dataset).then(function (confirmou) {
            if (!confirmou) {
                return;
            }

            formulario.dataset.confirmado = "true";

            if (typeof formulario.requestSubmit === "function") {
                formulario.requestSubmit(enviadoPor || undefined);
            } else {
                formulario.submit();
            }
        });
    }, true);


    /* ---------- 6. Proteção contra clique duplo ---------- */

    document.addEventListener("submit", function (evento) {
        var formulario = evento.target;

        // Se a validação do navegador ou o diálogo cancelaram o envio, não trava nada.
        if (evento.defaultPrevented ||
            !(formulario instanceof HTMLFormElement) ||
            formulario.method.toLowerCase() !== "post") {
            return;
        }

        // Depois do envio começar, desativa os botões. O timeout garante que o
        // navegador já montou os dados do formulário.
        window.setTimeout(function () {
            formulario.querySelectorAll("button[type=submit], button:not([type])").forEach(function (botao) {
                if (botao.disabled) {
                    return;
                }

                botao.disabled = true;
                botao.dataset.travadoPeloEnvio = "true";

                if (botao.dataset.loadingText) {
                    botao.dataset.textoOriginal = botao.textContent;
                    botao.textContent = botao.dataset.loadingText;
                }
            });
        }, 0);
    });

    // Voltar pelo histórico do navegador pode trazer a página com os botões travados.
    window.addEventListener("pageshow", function (evento) {
        if (!evento.persisted) {
            return;
        }

        document.querySelectorAll("[data-travado-pelo-envio]").forEach(function (botao) {
            botao.disabled = false;
            delete botao.dataset.travadoPeloEnvio;

            if (botao.dataset.textoOriginal) {
                botao.textContent = botao.dataset.textoOriginal;
            }
        });

        document.querySelectorAll("form[data-confirmado]").forEach(function (formulario) {
            delete formulario.dataset.confirmado;
        });
    });


    /* ---------- 7. Avisos de sucesso que somem sozinhos ---------- */

    document.querySelectorAll(".alert-success[role=status]").forEach(function (aviso) {
        window.setTimeout(function () {
            aviso.classList.add("alert-saindo");

            window.setTimeout(function () {
                aviso.hidden = true;
            }, 450);
        }, 6000);
    });
})();
