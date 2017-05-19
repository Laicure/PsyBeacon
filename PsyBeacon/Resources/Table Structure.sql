CREATE TABLE [dbo].[PsyMain](
	[IDx] [varchar](128) NOT NULL primary key,
	[Username] [varchar](64) NOT NULL,
	[Name] [nvarchar](192) NOT NULL,
	[WinTitle] [nvarchar](max) NULL,
	[WinProcess] [nvarchar](max) NULL,
	[TimeLogged] [datetime2] NOT NULL
)