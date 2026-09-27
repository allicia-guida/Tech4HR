(function () {
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