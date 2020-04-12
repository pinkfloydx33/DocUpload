CREATE TABLE [dbo].[Documents]
(
	[DocumentId] BIGINT NOT NULL IDENTITY(1,1),
	[FileName] NVARCHAR(256) NOT NULL,
	[Title] NVARCHAR(512) NOT NULL,
	[Description] NVARCHAR(1024) NULL,
	[CreateDate] DATETIMEOFFSET NOT NULL DEFAULT(GETUTCDATE()),
	[FileSize] BIGINT NOT NULL,
	[DocumentType] INT NOT NULL,
	[Owner] NVARCHAR(512) NOT NULL DEFAULT N'unknown@anonymo.us', 
    CONSTRAINT [PK_Documents] PRIMARY KEY CLUSTERED ([DocumentId]),
	CONSTRAINT [UX_Documents_Title_Owner] UNIQUE([Title] ASC, [Owner] ASC),
	CONSTRAINT [FK_Documents_DocumentType] FOREIGN KEY([DocumentType]) REFERENCES [dbo].[DocumentTypes]([DocumentTypeId])
)
GO

CREATE NONCLUSTERED INDEX [IX_Documents_Owner] ON [dbo].[Documents]([Owner])
GO
