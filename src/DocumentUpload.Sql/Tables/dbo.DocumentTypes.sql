CREATE TABLE [dbo].[DocumentTypes]
(
	[DocumentTypeId] INT NOT NULL,
	[Name] NVARCHAR(50) NOT NULL,
	CONSTRAINT PK_DocumentType PRIMARY KEY CLUSTERED([DocumentTypeId])
)