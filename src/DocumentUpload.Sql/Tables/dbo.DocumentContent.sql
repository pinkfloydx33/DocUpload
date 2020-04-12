CREATE TABLE [dbo].[DocumentContent]
(
	[DocumentId] BIGINT NOT NULL,
	[Content] VARBINARY(MAX) NOT NULL,
	CONSTRAINT PK_DocumentContent PRIMARY KEY CLUSTERED ([DocumentId]),
	CONSTRAINT FK_DocumentContent_Documents FOREIGN KEY([DocumentId]) REFERENCES [dbo].[Documents]([DocumentId]) ON DELETE CASCADE
)