/****** Object:  Table [dbo].[EventLog]    Script Date: 30.01.2020 18:31:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EventLog](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[source] [nvarchar](50) NOT NULL,
	[type] [nvarchar](10) NOT NULL,
	[logGroup] [nvarchar](50) NULL,
	[message] [nvarchar](300) NOT NULL,
	[timestamp] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_log] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[FunFactSubscriber]    Script Date: 30.01.2020 18:31:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FunFactSubscriber](
	[chatId] [nvarchar](30) NOT NULL,
	[nextUpdateOn] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_FunFactSubscriber] PRIMARY KEY CLUSTERED 
(
	[chatId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[RequestLog]    Script Date: 30.01.2020 18:31:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RequestLog](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[timestamp] [datetime2](0) NOT NULL,
	[requestJson] [nvarchar](2000) NOT NULL,
 CONSTRAINT [PK_requestLog] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

