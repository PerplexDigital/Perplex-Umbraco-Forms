using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerplexUmbraco.Forms.Code
{
    public static class Constants
    {
        /// <summary>
        /// Creates the perplexUmbracoUser table on application startup
        /// if it does not exist.
        /// </summary>
        public const string SQL_CREATE_PERPLEX_USER_TABLE_IF_NOT_EXISTS = @"
            IF OBJECT_ID('perplexUmbracoUser', 'U') IS NULL
            BEGIN
	            CREATE TABLE [dbo].[perplexUmbracoUser](
	            [id] [int] IDENTITY(1,1) NOT NULL,
	            [userId] [int] NOT NULL,
	            [formsStartNode] [varchar](36) NOT NULL,
	            CONSTRAINT [PK_perplexUmbracoUser] PRIMARY KEY CLUSTERED ( [id] ASC )
	            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	            ) ON [PRIMARY]
	
	            ALTER TABLE [dbo].[perplexUmbracoUser]  WITH CHECK ADD  CONSTRAINT [FK_perplexUmbracoUser_umbracoUser] FOREIGN KEY([userId])
	            REFERENCES [dbo].[umbracoUser] ([id])	

	            ALTER TABLE [dbo].[perplexUmbracoUser] CHECK CONSTRAINT [FK_perplexUmbracoUser_umbracoUser]	
            END";
    }
}
