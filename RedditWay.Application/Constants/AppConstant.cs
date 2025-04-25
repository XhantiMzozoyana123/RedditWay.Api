using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RedditWay.Application.Constants
{
    public static class AppConstant
    {
        public static readonly string appName = "Reddit Way";
        public static readonly string baseUrl = "https://redditway.com";
        public static readonly string webHostUrl = baseUrl + "/user";
        public static readonly string appHostUrl = baseUrl + "/user";
        public static readonly string baseExternal = "https://www.reddit.com";
        public static readonly string googleAiApiKey = "";
        public static readonly string youtubeDataApiKey = "";

        public static string RemoveAsterisk(string input)
        {
            return input.Replace("*", string.Empty);
        }

        public static string RemoveHashTags(string input)
        {
            return input.Replace("#", string.Empty);
        }

        public static string ConvertStringToHtml(string input, int id, bool tracking)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Escape special characters for HTML
            string encodedText = HttpUtility.HtmlEncode(input);

            // Convert new lines to <br> tags
            encodedText = encodedText.Replace(Environment.NewLine, "<br>");
            encodedText = encodedText.Replace("\n", "<br>");


            // Create a basic HTML structure for an email without a footer
            string htmlTemplate = $@"
                <!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            font-size: 14px;
                            line-height: 1.6;
                            color: #333;
                        }}
                    </style>
                </head>
                <body>
                    <div class=""email-content"">
                        {encodedText}
                    </div>
                    {InvisibleImageForHtml(id, tracking)}
                </body>
                </html>
            ";

            return htmlTemplate;
        }

        public static string InvisibleImageForHtml(int id, bool tracking)
        {
            string imgTag = string.Empty;

            if (tracking)
            {
                //The id is the actuall contact id of the person being emailed...
                string url = webHostUrl + $"tracker/track-open?contactid={id}";
                return imgTag = $"<img src='{url}' alt='' width='1' height='1' style='display:none;' />";
            }
            else
            {
                return "";
            }
        }

        public static string GenerateRandomString()
        {
            // Creating object of random class 
            Random rand = new Random();

            // Choosing the size of string 
            // Using Next() string 
            int stringlen = rand.Next(4, 10);
            int randValue;
            string str = "";
            char letter;
            for (int i = 0; i < stringlen; i++)
            {

                // Generating a random number. 
                randValue = rand.Next(0, 26);

                // Generating random character by converting 
                // the random number into character. 
                letter = Convert.ToChar(randValue + 65);

                // Appending the letter to string. 
                str = str + letter;
            }

            return str;
        }

        public static int ConvertMinutesToMilliseconds(int minutes)
        {
            return minutes * 60 * 1000;
        }
    }

}
