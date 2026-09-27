(function () {
    const toggleAcessibilidade = document.getElementById("toggle-acessibilidade");
    const body = document.body;

    if (toggleAcessibilidade) {
        const aplicarModoAcessibilidade = function (ativo) {
            body.classList.toggle("accessibility-daltonic", ativo);
            toggleAcessibilidade.setAttribute("aria-pressed", String(ativo));
            toggleAcessibilidade.setAttribute(
                "aria-label",
                ativo ? "Desativar contraste visual" : "Ativar contraste visual"
            );
            toggleAcessibilidade.title = ativo ? "Desativar contraste visual" : "Ativar contraste visual";
            toggleAcessibilidade.textContent = "👁️";
            localStorage.setItem("tech4hr-visual-contrast", ativo ? "true" : "false");
        };

        localStorage.removeItem("tech4hr-daltonic-mode");
        localStorage.setItem("tech4hr-visual-contrast", "false");
        aplicarModoAcessibilidade(false);

        toggleAcessibilidade.addEventListener("click", function () {
            const ativado = body.classList.contains("accessibility-daltonic");
            aplicarModoAcessibilidade(!ativado);
        });
    }

    const buscaFuncionario = document.getElementById("buscaFuncionario");
    const filtroStatusFuncionario = document.getElementById("filtroStatusFuncionario");
    const tabelaFuncionarios = document.getElementById("tabelaFuncionarios");

    if (!tabelaFuncionarios) {
        return;
    }

    const aplicarFiltros = function () {
        const termoBusca = (buscaFuncionario ? buscaFuncionario.value : "").trim().toLowerCase();
        const statusSelecionado = filtroStatusFuncionario ? filtroStatusFuncionario.value : "todos";

        const linhas = tabelaFuncionarios.querySelectorAll("tbody tr");

        linhas.forEach(function (linha) {
            const textoLinha = linha.textContent.toLowerCase();
            const statusLinha = (linha.dataset.status || "").toLowerCase();

            const correspondeBusca = !termoBusca || textoLinha.includes(termoBusca);
            const correspondeStatus =
                statusSelecionado === "todos" ||
                (statusSelecionado === "ativos" && statusLinha === "ativo") ||
                (statusSelecionado === "inativos" && statusLinha === "inativo");

            linha.style.display = correspondeBusca && correspondeStatus ? "" : "none";
        });
    };

    if (buscaFuncionario) {
        buscaFuncionario.addEventListener("input", aplicarFiltros);
    }

    if (filtroStatusFuncionario) {
        filtroStatusFuncionario.addEventListener("change", aplicarFiltros);
    }
})();