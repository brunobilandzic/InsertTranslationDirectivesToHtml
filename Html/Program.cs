using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Html
{
   
    class Program
    {
        
        static void Main(string[] args)
        {
            InsertDirectivesToHtmlFiles();
        }

        private static void RegexHell()
        {
            // Function to test various regexes..Ignore
            var tests = new string[] { "not angular. dont { match", "{{wait}}", "Wait {{What is}} this on {{hello}}" };

            Regex angularRegex = GetAngularRegex();

            foreach (var test in tests)
            {
                Console.WriteLine(test);
                
                foreach (var match in angularRegex.Matches(test))
                {
                    Console.WriteLine(match);
                }             
            }
            Console.ReadKey();
        }

        private static void InsertDirectivesToHtmlFiles()
        {
            // Update read path here
            var PathToHtmlFiles = @"C:\Users\bbilandzic\source\repos\Html\Html\HtmlFiles";

            var HtmlFileNames = Directory.GetFiles(PathToHtmlFiles, "file.html", SearchOption.AllDirectories);

            foreach (var item in HtmlFileNames)
            {
                InserDirectiveToFile(item);
            }


            Console.ReadKey();
        }



        private static void InserDirectiveToFile(string filename)
        {
            var doc = new HtmlDocument();
            doc.Load(filename);

            var htmlTextNodes = SelectAllTextNodes(doc);
            var htmlNodes = doc.DocumentNode.Descendants();

            Console.WriteLine($"\n\n------------FileName: {filename}---------------\n");

            foreach (var htmlNode in htmlTextNodes)
            {
                ManipulateNodeContainingOnlyText(htmlNode);
            }

            foreach(var htmlNode in htmlNodes)
            {
                TranslateNodeAttributes(htmlNode);
            }

            // Update write path here
            using (FileStream fileStream = File.Create(@"C:\Users\bbilandzic\source\repos\Html\Html\HtmlFiles\fileResult.html"))
            {
                doc.Save(fileStream);
            }     

        }

        private static void TranslateNodeAttributes(HtmlNode htmlNode)
        {
            foreach (var attribute in htmlNode.Attributes)
            {
                // should match upon a list of attributes we want to translate
                if (attribute.Name == "my-tooltip" || attribute.Name == "placeholder")
                {
                    if(IsContainingAngular(attribute.Value) == false)
                    {
                        attribute.Value = "{{'" + attribute.Value + "' | translate}}"; 
                    } else
                    {
                        // i dont know what we should do here
                        // case when html attribute contains some angular like {{}} stuff
                    }
                }
            }
        }

        private static IEnumerable<HtmlNode> SelectAllTextNodes(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectNodes("//text()[normalize-space(.) != '']").Where(n => n.ParentNode.Name != "script" && n.ParentNode.Name != "style");
        }

        private static void ManipulateNodeContainingOnlyText(HtmlNode htmlTextNode)
        {
            Console.WriteLine(htmlTextNode.InnerText.Trim());
            WrapInSpan(htmlTextNode);
        }

        private static string WrapTextInTranslationSpan(string text)
        {
            return String.Format("<span trasnlate=\"{0}\" translate-default=\"{1}\"></span>", text, text);
        }

        private static void WrapInSpan(HtmlNode htmlTextNode)
        {
            var nodeText = htmlTextNode.InnerText.Trim();
            if (String.IsNullOrEmpty(nodeText)) return;
            if (IsContainingAngular(nodeText))
            {
                // manage angular contaminated text translation
                var textToTranslate = nodeText.Split(BuildRegexMatchesToArray(GetAngularRegex().Matches(nodeText)), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < textToTranslate.Length; i++)
                {
                    var translatedText = WrapTextInTranslationSpan(textToTranslate[i].Trim());
                    Console.WriteLine($"Translating '{textToTranslate[i]}' to '{translatedText}'");
                    nodeText = nodeText.Replace(textToTranslate[i]," " + translatedText + " ");
                }
                Console.WriteLine($"Inner Text:\n{nodeText}");
                htmlTextNode.InnerHtml = nodeText;
            } else
            {
                htmlTextNode.InnerHtml = WrapTextInTranslationSpan(htmlTextNode.InnerText.Trim());
            }
        }

        private static bool IsContainingAngular(string nodeText)
        {
            var angularRegex = GetAngularRegex();
            return angularRegex.IsMatch(nodeText);
        }

        private static Regex GetAngularRegex()
        {
            string angularRegexPattern = @"\{\{[^\{\}]*\}\}";
            return new Regex(angularRegexPattern);
        }

        private static string [] BuildRegexMatchesToArray(MatchCollection matches)
        {
            var matchList = new List<string>();
            foreach (var match in matches)
            {
                matchList.Add(match.ToString());
            }
            return matchList.ToArray();
         }
    }
}

