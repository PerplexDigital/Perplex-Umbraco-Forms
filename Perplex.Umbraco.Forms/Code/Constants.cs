using PerplexUmbraco.Forms.Code.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerplexUmbraco.Forms.Code
{
    public static class Constants
    {
        /// <summary>
        /// All Field Type Ids
        /// </summary>
        public enum EnmFieldTypeId
        {
            [Description("3e170f26-1fcb-4f60-b5d2-1aa2723528fd")]
            PerplexFileUpload,
            [Description("11fff56b-7e0e-4bfc-97ba-b5126158d33d")]
            PerplexImageUpload,
            [Description("9ead6835-57db-418b-ae2b-528f8db375a0")]
            PerplexTextField,
            [Description("8c38cb28-8018-4545-b939-d1166a96b916")]
            PerplexTextarea,
            [Description("9c804aa5-d7d6-42d8-b492-2e06101987ad")]
            PerplexRecaptcha,

            [Description("d5c0c390-ae9a-11de-a69e-666455d89593")]
            Checkbox,
            [Description("f8b4c3b8-af28-11de-9dd8-ef5956d89593")]
            Date,
            [Description("0dd29d42-a6a5-11de-a2f2-222256d89593")]
            Dropdown,
            [Description("3f92e01b-29e2-4a30-bf33-9df5580ed52c")]
            ShortAnswer,
            [Description("023f09ac-1445-4bcb-b8fa-ab49f33bd046")]
            LongAnswer,
            [Description("84a17cf8-b711-46a6-9840-0e4a072ad000")]
            FileUpload,
            [Description("fb37bc60-d41e-11de-aeae-37c155d89593")]
            Password,
            [Description("fab43f20-a6bf-11de-a28f-9b5755d89593")]
            MultipleChoice,
            [Description("903df9b0-a78c-11de-9fc1-db7a56d89593")]
            SingleChoice,
            [Description("e3fbf6c4-f46c-495e-aff8-4b3c227b4a98")]
            TitleAndDescription,
            [Description("4a2e8e12-9613-4720-9bcd-f9871262d6ac")]
            Recaptcha,
            [Description("da206cae-1c52-434e-b21a-4a7c198af877")]
            Hidden,
        }

        public const string PERPLEX_FIELDTYPE_ROOT_FOLDER = "~/Views/Partials/Forms/Fieldtypes/Perplex";
        public const string DATATYPE_ROOT_FOLDER = "~/App_Plugins/PerplexUmbracoForms";        

        /// <summary>
        /// Relative location of folders.json file
        /// </summary>
        public const string FOLDERS_DATA_FILE_PATH = DATATYPE_ROOT_FOLDER + "/data/folders.json";

        /// <summary>
        /// Relative location of PerplexUmbracoForms.config file
        /// </summary>
        public const string CONFIGURATION_FILE_PATH = DATATYPE_ROOT_FOLDER + "/PerplexUmbracoForms.config";

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
