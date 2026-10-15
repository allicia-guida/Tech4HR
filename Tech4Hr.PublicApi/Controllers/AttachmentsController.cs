using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.PublicApi.Data;
using Tech4Hr.PublicApi.Models;
using Tech4Hr.PublicApi.Services;

namespace Tech4Hr.PublicApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/attachments")]
public sealed class AttachmentsController(
    PublicApiDbContext db,
    ICurrentUser currentUser,
    IAuditService audit,
    IConfiguration configuration) : ControllerBase
{
    private const long MaximumSize = 5 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, byte[]> Signatures = new Dictionary<string, byte[]>
    {
        ["application/pdf"] = "%PDF-"u8.ToArray(),
        ["image/jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        ["image/png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
    };

    [HttpPost]
    [RequestSizeLimit(MaximumSize)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] Guid? pointCorrectionId,
        [FromForm] Guid? absenceRequestId,
        CancellationToken cancellationToken)
    {
        if ((pointCorrectionId is null) == (absenceRequestId is null))
            return BadRequest(new { message = "Informe uma correção ou uma ausência." });
        if (file.Length is <= 0 or > MaximumSize || !Signatures.TryGetValue(file.ContentType, out var signature))
            return BadRequest(new { message = "Arquivo inválido. Envie PDF, JPEG ou PNG com até 5 MB." });
        var ownsReference = pointCorrectionId is not null
            ? await db.PointCorrections.AnyAsync(x => x.Id == pointCorrectionId && x.UserId == currentUser.UserId, cancellationToken)
            : await db.AbsenceRequests.AnyAsync(x => x.Id == absenceRequestId && x.UserId == currentUser.UserId, cancellationToken);
        if (!ownsReference) return NotFound();

        await using var source = file.OpenReadStream();
        var header = new byte[signature.Length];
        if (await source.ReadAsync(header, cancellationToken) != signature.Length || !header.SequenceEqual(signature))
            return BadRequest(new { message = "O conteúdo do arquivo não corresponde ao tipo informado." });
        source.Position = 0;

        var storageRoot = Path.GetFullPath(configuration["Storage:AttachmentPath"] ?? "attachments");
        Directory.CreateDirectory(storageRoot);
        var storedName = $"{Guid.NewGuid():N}.bin";
        var target = Path.Combine(storageRoot, storedName);
        await using (var destination = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
        {
            await source.CopyToAsync(destination, cancellationToken);
        }

        var attachment = new Attachment
        {
            UserId = currentUser.UserId,
            PointCorrectionId = pointCorrectionId,
            AbsenceRequestId = absenceRequestId,
            OriginalName = Path.GetFileName(file.FileName),
            StoredName = storedName,
            ContentType = file.ContentType,
            SizeBytes = file.Length
        };
        db.Attachments.Add(attachment);
        audit.Add(currentUser.UserId, "UPLOAD", nameof(Attachment), attachment.Id);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            System.IO.File.Delete(target);
            throw;
        }
        return CreatedAtAction(nameof(Download), new { id = attachment.Id }, new { attachment.Id, attachment.OriginalName, attachment.ContentType, attachment.SizeBytes });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var attachment = await db.Attachments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (attachment is null) return NotFound();
        var elevated = User.IsInRole(Roles.Administrador) || User.IsInRole(Roles.Gestor);
        if (attachment.UserId != currentUser.UserId && !elevated) return Forbid();
        var storageRoot = Path.GetFullPath(configuration["Storage:AttachmentPath"] ?? "attachments");
        var target = Path.GetFullPath(Path.Combine(storageRoot, attachment.StoredName));
        if (!target.StartsWith(storageRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(target)) return NotFound();
        return PhysicalFile(target, attachment.ContentType, attachment.OriginalName, enableRangeProcessing: false);
    }
}
