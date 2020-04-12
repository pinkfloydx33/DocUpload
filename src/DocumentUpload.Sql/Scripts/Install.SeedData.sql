IF NOT EXISTS (SELECT TOP (1) 1 FROM [dbo].[DocumentTypes])
BEGIN
	
	INSERT INTO [dbo].[DocumentTypes] ([DocumentTypeId], [Name])
	VALUES 
		(0, N'Unknown')
	   ,(1, N'Text')
	   ,(2, N'Image')
	   ,(3, N'PDF');

END