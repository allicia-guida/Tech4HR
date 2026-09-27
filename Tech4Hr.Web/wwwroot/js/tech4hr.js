const buscaFuncionario =
    document.getElementById("buscaFuncionario");

const tabelaFuncionarios =
    document.getElementById("tabelaFuncionarios");

if (buscaFuncionario && tabelaFuncionarios) {

    buscaFuncionario.addEventListener("input", function () {

        const busca =
            this.value.toLowerCase().trim();

        const linhas =
            tabelaFuncionarios.querySelectorAll("tbody tr");

        linhas.forEach(function (linha) {

            const conteudo =
                linha.textContent.toLowerCase();

            linha.style.display =
                conteudo.includes(busca)
                    ? ""
                    : "none";
        });
    });
}