using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Extensions;

namespace PerplexUmbraco.Forms.Code
{
    public static class FieldTypeHelpers
    {
        /// <summary>
        /// Retrieves a specific value from a Field's setting Object
        /// When the key does not exist, a default value is returned
        /// </summary>
        /// <param name="field">The Field to retrieve the Setting value from</param>
        /// <param name="key">The key of the Setting</param>
        /// <param name="defaultValue">The value to return when the key is not present</param>
        /// <returns></returns>
        public static T GetSettingValue<T>(Field field, string key, Func<string, T> transform, T defaultValue = default(T))
        {
            string value = null;

            if (!field.Settings.TryGetValue(key, out value) || string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try { return transform(value); }
            catch { return defaultValue; }
        }

        public static string GetSettingValue(Field field, string key, string defaultValue = "")
        {
            return GetSettingValue(field, key, s => s, defaultValue);
        }

        public static IEnumerable<string> ValidateAllowedFileTypes(Field field, HttpContextBase context, IEnumerable<string> allowedFileTypes, string errorMessage = "")
        {
            List<string> filenames = context.Request.Files.GetMultiple(field.Id.ToString())
                .Select(f => f.FileName)
                .Where(filename => !string.IsNullOrEmpty(filename))
                .ToList();

            foreach (var filename in filenames)
            {
                if (!allowedFileTypes.Contains(Path.GetExtension(filename.ToString()).Replace(".", "").ToLower()))
                {
                    return new []
                    {
                        string.Format(StringExtensions.ParsePlaceHolders(errorMessage), field.Caption)
                    };
                }
            }

            return Enumerable.Empty<string>();
        }

        public static IEnumerable<string> ValidateMaxFileSize(Field field, HttpContextBase context, decimal? maxFileSize, string errorMessage = "")
        {
            IList<HttpPostedFileBase> files = context.Request.Files.GetMultiple(field.Id.ToString());

            // Check the file size
            if (maxFileSize.HasValue && maxFileSize > 0)
            {
                decimal maxBytes = maxFileSize.Value * 1024 * 1024;

                // TODO: Should we perhaps check the sum of all file sizes rather than each individual file size?
                // With a File Upload set to Multiple you could otherwise just send a bunch
                // of smaller files to more or less avoid the file size limit and still send a lot of data to the server
                // Probably best to make it configurable => "Apply Max File Size to all files combined?"

                foreach(HttpPostedFileBase file in files)
                {
                    // ContentLength is in bytes
                    if (file.ContentLength > maxBytes)
                    {
                        // Reset the uploaded files
                        return new[]
                        {
                            string.Format(StringExtensions.ParsePlaceHolders(errorMessage), field.Caption)
                        };
                    }
                }
            }

            return Enumerable.Empty<string>();
        }
    }
}
