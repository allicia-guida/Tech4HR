using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Web.Controllers;

public class PontosController : Controller
{
    private readonly IPontoService _pontoService;
    private readonly IFuncionarioService _funcionarioService;

    public PontosController(
        IPontoService pontoService,
        IFuncionarioService funcionarioService)
    {
        _pontoService = pontoService;
        _funcionarioService = funcionarioService;
    }

    private bool UsuarioEhAdminOuOperacional()
    {
        var nivel = HttpContext.Session.GetString("UsuarioNivel");
        return string.Equals(nivel, "ADMIN", StringComparison.OrdinalIgnoreCase)
            || string.Equals(nivel, "OPERACIONAL", StringComparison.OrdinalIgnoreCase);
    }

    private bool UsuarioLogado()
    {
        return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("AuthToken"));
    }

    private async Task<IReadOnlyList<PontoApiResponse>> CarregarRegistrosAsync(
        string token,
        int? idFuncionario,
        DateTime? dataInicio,
        DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        var registros = await _pontoService.ConsultarAsync(
            token,
            idFuncionario,
            dataInicio,
            dataFim,
            cancellationToken);

        return registros
            .OrderByDescending(p => p.DataPonto)
            .ToList();
    }

    private static string FormatarDataHora(DateTime? valor)
    {
        return valor.HasValue
            ? valor.Value.ToString("yyyy-MM-ddTHH:mm:ssK")
            : string.Empty;
    }

    private static byte[] GerarXmlPontos(IReadOnlyList<PontoApiResponse> registros)
    {
        var itens = registros.Select(registro =>
            new XElement("Registro",
                new XElement("IdPonto", registro.IdPonto),
                new XElement("IdFuncionario", registro.IdFuncionario),
                new XElement("Funcionario", registro.Funcionario.NomeCompleto),
                new XElement("DataPonto", registro.DataPonto.ToString("yyyy-MM-dd")),
                new XElement("Entrada", FormatarDataHora(registro.Entrada)),
                new XElement("SaidaAlmoco", FormatarDataHora(registro.SaidaAlmoco)),
                new XElement("EntradaAlmoco", FormatarDataHora(registro.EntradaAlmoco)),
                new XElement("Saida", FormatarDataHora(registro.Saida))));

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("RegistrosPonto",
                new XAttribute("GeradoEm", DateTimeOffset.Now.ToString("o")),
                itens));

        return Encoding.UTF8.GetBytes(document.ToString(SaveOptions.None));
    }

    private static string ObterReferenciaColuna(int indice)
    {
        var coluna = string.Empty;
        var valor = indice + 1;

        while (valor > 0)
        {
            valor--;
            coluna = (char)('A' + (valor % 26)) + coluna;
            valor /= 26;
        }

        return coluna;
    }

    private static string FormatarValorExcel(string? valor)
    {
        return string.IsNullOrEmpty(valor) ? string.Empty : System.Net.WebUtility.HtmlEncode(valor);
    }

    private static string GerarLinhaExcel(int numeroLinha, params string[] valores)
    {
        var celulas = new StringBuilder();

        for (var indice = 0; indice < valores.Length; indice++)
        {
            var coluna = ObterReferenciaColuna(indice);
            var valor = FormatarValorExcel(valores[indice]);
            celulas.Append($"<c r=\"{coluna}{numeroLinha}\" t=\"inlineStr\"><is><t>{valor}</t></is></c>");
        }

        return $"<row r=\"{numeroLinha}\">{celulas}</row>";
    }

    private static byte[] GerarXlsxPontos(IReadOnlyList<PontoApiResponse> registros)
    {
        using var stream = new MemoryStream();

        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            static void AdicionarEntrada(ZipArchive archive, string path, string content)
            {
                var entry = archive.CreateEntry(path);
                using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
                writer.Write(content);
            }

            AdicionarEntrada(archive, "[Content_Types].xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                "<Override PartName=\"/docProps/core.xml\" ContentType=\"application/vnd.openxmlformats-package.core-properties+xml\"/>" +
                "<Override PartName=\"/docProps/app.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.extended-properties+xml\"/>" +
                "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
                "</Types>");

            AdicionarEntrada(archive, "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties\" Target=\"docProps/core.xml\"/>" +
                "<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties\" Target=\"docProps/app.xml\"/>" +
                "</Relationships>");

            AdicionarEntrada(archive, "docProps/core.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<cp:coreProperties xmlns:cp=\"http://schemas.openxmlformats.org/package/2006/metadata/core-properties\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:dcmitype=\"http://purl.org/dc/dcmitype/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                "<dc:creator>Tech4Hr</dc:creator><cp:lastModifiedBy>Tech4Hr</cp:lastModifiedBy><dcterms:created xsi:type=\"dcterms:W3CDTF\">" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "</dcterms:created><dcterms:modified xsi:type=\"dcterms:W3CDTF\">" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "</dcterms:modified></cp:coreProperties>");

            AdicionarEntrada(archive, "docProps/app.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Properties xmlns=\"http://schemas.openxmlformats.org/officeDocument/2006/extended-properties\" xmlns:vt=\"http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes\"><Application>Tech4Hr</Application></Properties>");

            AdicionarEntrada(archive, "xl/styles.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"/>\n");

            var linhas = new List<string>
            {
                GerarLinhaExcel(1, "Funcionário", "Data", "Entrada", "Saída almoço", "Retorno almoço", "Saída")
            };

            var indiceLinha = 2;
            foreach (var registro in registros)
            {
                linhas.Add(GerarLinhaExcel(
                    indiceLinha,
                    registro.Funcionario.NomeCompleto,
                    registro.DataPonto.ToString("dd/MM/yyyy"),
                    registro.Entrada?.ToString("HH:mm") ?? "-",
                    registro.SaidaAlmoco?.ToString("HH:mm") ?? "-",
                    registro.EntradaAlmoco?.ToString("HH:mm") ?? "-",
                    registro.Saida?.ToString("HH:mm") ?? "-"));

                indiceLinha++;
            }

            var sheetXml = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                "<sheetData>" + string.Concat(linhas) + "</sheetData></worksheet>";

            AdicionarEntrada(archive, "xl/worksheets/sheet1.xml", sheetXml);

            AdicionarEntrada(archive, "xl/workbook.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                "<sheets><sheet name=\"Pontos\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");

            AdicionarEntrada(archive, "xl/_rels/workbook.xml.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
                "</Relationships>");
        }

        return stream.ToArray();
    }

    public async Task<IActionResult> Index(
        int? idFuncionario,
        DateTime? dataInicio,
        DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        if (!UsuarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!UsuarioEhAdminOuOperacional())
        {
            TempData["Erro"] = "Acesso restrito a administradores e operacionais.";
            return RedirectToAction("Index", "Dashboard");
        }

        var token = HttpContext.Session.GetString("AuthToken");

        try
        {
            var funcionarios =
                await _funcionarioService.ListarAsync(
                    token!,
                    cancellationToken);

            var registros = await CarregarRegistrosAsync(
                token!,
                idFuncionario,
                dataInicio,
                dataFim,
                cancellationToken);

            var model = new PontosConsultaViewModel
            {
                IdFuncionario = idFuncionario,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Funcionarios = funcionarios,
                Registros = registros
            };

            return View(model);
        }
        catch (HttpRequestException)
        {
            TempData["Erro"] = "Não foi possível carregar os registros de ponto.";

            return View(new PontosConsultaViewModel
            {
                IdFuncionario = idFuncionario,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Funcionarios = Array.Empty<FuncionarioApiResponse>()
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportarXml(
        int? idFuncionario,
        DateTime? dataInicio,
        DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        if (!UsuarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!UsuarioEhAdminOuOperacional())
        {
            TempData["Erro"] = "Acesso restrito a administradores e operacionais.";
            return RedirectToAction("Index", "Dashboard");
        }

        var token = HttpContext.Session.GetString("AuthToken");
        var registros = await CarregarRegistrosAsync(
            token!,
            idFuncionario,
            dataInicio,
            dataFim,
            cancellationToken);

        var bytes = GerarXmlPontos(registros);
        return File(
            bytes,
            "application/xml",
            $"pontos_{DateTime.Now:yyyyMMdd_HHmmss}.xml");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarXlsx(
        int? idFuncionario,
        DateTime? dataInicio,
        DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        if (!UsuarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!UsuarioEhAdminOuOperacional())
        {
            TempData["Erro"] = "Acesso restrito a administradores e operacionais.";
            return RedirectToAction("Index", "Dashboard");
        }

        var token = HttpContext.Session.GetString("AuthToken");
        var registros = await CarregarRegistrosAsync(
            token!,
            idFuncionario,
            dataInicio,
            dataFim,
            cancellationToken);

        var bytes = GerarXlsxPontos(registros);
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"pontos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}

