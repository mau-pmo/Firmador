namespace Firmador.Core.Documentos;

public sealed record DocumentoParticipantes(
    int DocumentoId,
    ParticipanteDocumento? Creador,
    ParticipanteDocumento? Editor,
    IReadOnlyList<RevisorDocumento> Revisores,
    IReadOnlyList<FirmanteDocumento> Firmantes);

public sealed record ParticipanteDocumento(string? Nombre);

public sealed record RevisorDocumento(int Orden, string? Estado, DateTimeOffset? RevisadoAt, string? Nombre);

public sealed record FirmanteDocumento(int Orden, string? Estado, DateTimeOffset? FirmadoAt, string? Tipo, string? Nombre);
