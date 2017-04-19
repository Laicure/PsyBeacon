CREATE TABLE [dbo].[PsyMain](
	[IDx] [varchar](128) NOT NULL,
	[Username] [varchar](64) NOT NULL,
	[Name] [nvarchar](192) NOT NULL,
	[WinTitle] [nvarchar](max) NULL,
	[WinProcess] [nvarchar](max) NULL,
	[TimeLogged] [datetime] NOT NULL
)