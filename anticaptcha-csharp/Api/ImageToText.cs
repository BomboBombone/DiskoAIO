using Anticaptcha_example.ApiResponse;
using Anticaptcha_example.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace Anticaptcha_example.Api
{
    public class ImageToText : AnticaptchaBase, IAnticaptchaTaskProtocol
    {
        public enum NumericOption
        {
            NoRequirements,
            NumbersOnly,
            AnyLettersExceptNumbers
        }

        public ImageToText()
        {
            BodyBase64 = "";
            Phrase = false;
            Case = false;
            Numeric = NumericOption.NoRequirements;
            Math = 0;
            MinLength = 0;
            MaxLength = 0;
        }

        public string BodyBase64 { private get; set; }

        public string FilePath
        {
            set
            {
                using (WebClient webClient = new WebClient())
                {
                    byte[] data = webClient.DownloadData(value);

                    using (MemoryStream mem = new MemoryStream(data))
                    {
                        using (var image = Image.FromStream(mem))
                        {
                            // If you want it as Png
                            using (MemoryStream m = new MemoryStream())
                            {
                                image.Save(m, image.RawFormat);
                                byte[] imageBytes = m.ToArray();

                                // Convert byte[] to Base64 String
                                string base64String = Convert.ToBase64String(imageBytes);
                                BodyBase64 = base64String;
                            }
                        }
                    }

                }
            }
        }

        public bool Phrase { private get; set; }
        public bool Case { private get; set; }
        public NumericOption Numeric { private get; set; }
        public int Math { private get; set; }
        public int MinLength { private get; set; }
        public int MaxLength { private get; set; }

        public override JObject GetPostData()
        {
            if (string.IsNullOrEmpty(BodyBase64))
            {
                return null;
            }

            return new JObject
            {
                {"type", "ImageToTextTask"},
                {"body", BodyBase64.Replace("\r", "").Replace("\n", "")},
                {"phrase", Phrase},
                {"case", Case},
                {"numeric", Numeric.Equals(NumericOption.NoRequirements) ? 0 : Numeric.Equals(NumericOption.NumbersOnly) ? 1 : 2},
                {"math", Math},
                {"minLength", MinLength},
                {"maxLength", MaxLength}
            };
        }

        public TaskResultResponse.SolutionData GetTaskSolution()
        {
            return TaskInfo.Solution;
        }
    }
}