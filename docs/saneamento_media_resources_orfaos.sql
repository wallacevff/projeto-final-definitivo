/*
Saneamento de MediaResources orfaos (registros sem arquivo no storage e sem referencias).

Uso:
1) Cole neste script os StoragePath faltantes no bucket (veja bloco INSERT).
2) Rode com @ApplyDelete = 0 para diagnostico.
3) Revise o resultado.
4) Rode com @ApplyDelete = 1 para excluir os IDs candidatos.
*/

SET NOCOUNT ON;

DECLARE @ApplyDelete BIT = 0;

IF OBJECT_ID('tempdb..#MissingStoragePaths') IS NOT NULL
    DROP TABLE #MissingStoragePaths;

CREATE TABLE #MissingStoragePaths
(
    StoragePath NVARCHAR(512) NOT NULL PRIMARY KEY
);

/*
Preencha com os caminhos faltantes no bucket.
Exemplo:
INSERT INTO #MissingStoragePaths (StoragePath) VALUES
('meajudaai/media/2026/03/273591e1b4464b418c7e9324a9d24c16.mp4'),
('meajudaai/media/2026/03/b836f1c3bca240f0a3778c6131af6ab1.mp4');
*/

;WITH ReferenceCount AS
(
    SELECT
        m.Id,
        UsageCount =
            (CASE WHEN EXISTS (SELECT 1 FROM Courses c WHERE c.ThumbnailMediaId = m.Id) THEN 1 ELSE 0 END) +
            (CASE WHEN EXISTS (SELECT 1 FROM ChatMessages cm WHERE cm.MediaResourceId = m.Id) THEN 1 ELSE 0 END) +
            (CASE WHEN EXISTS (SELECT 1 FROM ContentAttachments ca WHERE ca.MediaResourceId = m.Id) THEN 1 ELSE 0 END) +
            (CASE WHEN EXISTS (SELECT 1 FROM ActivityAttachments aa WHERE aa.MediaResourceId = m.Id) THEN 1 ELSE 0 END) +
            (CASE WHEN EXISTS (SELECT 1 FROM ForumPostAttachments fpa WHERE fpa.MediaResourceId = m.Id) THEN 1 ELSE 0 END) +
            (CASE WHEN EXISTS (SELECT 1 FROM SubmissionAttachments sa WHERE sa.MediaResourceId = m.Id) THEN 1 ELSE 0 END) +
            (CASE WHEN OBJECT_ID('ContentVideoAnnotations') IS NOT NULL
                        AND EXISTS (SELECT 1 FROM ContentVideoAnnotations cva WHERE cva.MediaResourceId = m.Id)
                   THEN 1 ELSE 0 END)
    FROM MediaResources m
),
Candidates AS
(
    SELECT
        m.Id,
        m.FileName,
        m.OriginalFileName,
        m.StoragePath,
        m.Sha256,
        m.CreatedAt
    FROM MediaResources m
    INNER JOIN #MissingStoragePaths ms
        ON ms.StoragePath = m.StoragePath
    INNER JOIN ReferenceCount rc
        ON rc.Id = m.Id
    WHERE rc.UsageCount = 0
)
SELECT
    c.Id,
    c.OriginalFileName,
    c.StoragePath,
    c.Sha256,
    c.CreatedAt
FROM Candidates c
ORDER BY c.CreatedAt DESC;

IF @ApplyDelete = 1
BEGIN
    DELETE m
    FROM MediaResources m
    INNER JOIN Candidates c
        ON c.Id = m.Id;

    SELECT @@ROWCOUNT AS DeletedRows;
END
ELSE
BEGIN
    SELECT COUNT(*) AS CandidateRows FROM Candidates;
END

