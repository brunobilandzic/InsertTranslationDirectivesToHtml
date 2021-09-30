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

        private static void InsertDirectivesToHtmlFiles()
        {
            foreach (var item in getHtmlFileNames())
            {
                InserDirectivesToFile(item);
            }

            Console.WriteLine("Finish");
            Console.ReadKey();
        }

        private static IEnumerable<string> getHtmlFileNames()
        {
            var fileList = new List<string>();

            foreach (var path in getDirectoryPaths())
            {
                fileList.AddRange(Directory.GetFiles(path, "*file.html", SearchOption.AllDirectories));
            }

            return fileList;
        }

        private static void InserDirectivesToFile(string filename)
        {
            if (filename.Contains("Vendor")) return;
            // Console.WriteLine($"Writing directives to {filename}");
            var doc = new HtmlDocument();
            doc.Load(filename);
            doc.OptionWriteEmptyNodes = true;
            doc.GlobalAttributeValueQuote = AttributeValueQuote.Initial;
            doc.OptionDefaultUseOriginalName = true;
            doc.OptionOutputOriginalCase = true;
            var htmlTextNodes = SelectAllTextNodes(doc);
            var htmlNodes = doc.DocumentNode.Descendants();

            if(htmlTextNodes != null)
                foreach (var htmlTextNode in htmlTextNodes)
                {
                    TranslateNodeText(htmlTextNode);
                }

            if(htmlNodes != null)
                foreach(var htmlNode in htmlNodes)
                {
                   TranslateNodeAttributes(htmlNode);
                }

            // Update write path here
            using (FileStream fileStream = File.Create(filename))
            {
                doc.Save(fileStream, Encoding.UTF8);
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
                        Console.WriteLine($"Translating {attribute.Name}");
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
            return doc.DocumentNode
                .SelectNodes("//text()[normalize-space(.) != '']")?
                .Where(n => n.ParentNode.Name != "script" && n.ParentNode.Name != "style");
        }

        private static string WrapTextInTranslationSpan(string text)
        {
            return String.Format("\n<span translate=\"{0}\" translate-default=\"{1}\"></span>\n", text, text);
        }

        private static void TranslateNodeText(HtmlNode htmlTextNode)
        {
            var nodeText = htmlTextNode.InnerText.Trim();
            if (ShouldTranslateNodeText(nodeText) == false) return;
            if (IsContainingAngular(nodeText))
            {
                string [] textToTranslate = nodeText.Split(
                    BuildRegexMatchesToArray(GetAngularRegex().Matches(nodeText)), 
                    StringSplitOptions.RemoveEmptyEntries
                    );
                for (int i = 0; i < textToTranslate.Length; i++)
                {
                    if (ShouldTranslateNodeText(textToTranslate[i].Trim()) == false) continue;
                    var translatedText = WrapTextInTranslationSpan(textToTranslate[i].Trim());
                    nodeText = nodeText.Replace(textToTranslate[i]," " + translatedText + " ");
                }
                htmlTextNode.InnerHtml = nodeText;
            } else
            {
                htmlTextNode.InnerHtml = WrapTextInTranslationSpan(htmlTextNode.InnerText.Trim());
            }
        }

        private static bool ShouldTranslateNodeText(string nodeText)
        {
            return (
                String.IsNullOrEmpty(nodeText) ||
                ContainsCSharpCode(nodeText) ||
                (nodeText.StartsWith('{') && IsContainingAngular(nodeText) == false) ||
                nodeText.StartsWith('}') ||
                IsNotWordOnlyRegex(nodeText)
                ) == false;            
        }
        private static string [] getDirectoryPaths()
        {
            return new string[]
            {
               @"C:\Users\bbilandzic\source\repos\Html\Html\HtmlFiles"
               // DIRECTORIES OF INTEREST HERE
            };
        }

        private static bool IsContainingAngular(string nodeText)
        {
            return GetAngularRegex().IsMatch(nodeText);
        }

        private static Regex GetAngularRegex()
        {
            return new Regex(@"\{\{[^\{\}]*\}\}");
        }

        private static bool IsNotWordOnlyRegex(string nodeText)
        {
            return new Regex(@"^\W+$").IsMatch(nodeText);
        }

        private static bool ContainsCSharpCode(string nodeText)
        {
            return new Regex(@"@").IsMatch(nodeText);
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

