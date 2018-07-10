using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Text.RegularExpressions;


public class Card
{
    public string Name, Cost, Img, Type, NumberOfSet, Stats, Rarity, NameENG, stylesheet, FileImg, Color, RuleText, FlavorText;
    public Int32 Number;
}

namespace mtg
{
    class Program
    {
        public static string DownLoadSet(string SetCode, string WZUrl, string MTGUrl, List<Card> Cards, int CountCards, int MaxSetCard)
        {
            string SetLocation = "c:/sets/" + SetCode + "/set";
            WebClient MTG = new WebClient();
            WebClient WZ = new WebClient();

            HtmlDocument htmlDocumentMTG = new HtmlDocument();
            HtmlDocument htmlDocumentWZ = new HtmlDocument();

            Console.Clear();
            Console.WriteLine("Для скачивания нажмите Y");
            string Y = Console.ReadLine();

            Directory.CreateDirectory("c:/sets/" + SetCode + "/");

            for (int i = 0; i < CountCards; i++)
            {
                Cards.Add(new Card());

                Console.Clear();
                Console.WriteLine("Начата обработка карт");
                Console.WriteLine("Идет обработка " + (i + 1) + " карты...");

                htmlDocumentMTG.LoadHtml(MTG.DownloadString(MTGUrl + (i + 1) + " & Grp=" + SetCode));

                var N = htmlDocumentMTG.DocumentNode.SelectSingleNode("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']" + "//h2").InnerText;
                Cards[i].Name = (N.Substring(N.IndexOf("//") + 3, N.Length - N.IndexOf("//") - 3)).Trim();
                Cards[i].NameENG = (N.Substring(0, N.IndexOf("//"))).Trim();
                Cards[i].Type = (htmlDocumentMTG.DocumentNode.SelectSingleNode("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']" + "//span[@class='rus']").InnerText).Substring(2);
                Cards[i].NumberOfSet = htmlDocumentMTG.DocumentNode.SelectSingleNode("//table[@class='NoteDiv U shadow']" + "//span[@class='bold2']").InnerText;
                
                switch (htmlDocumentMTG.DocumentNode.SelectNodes("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']")[4].InnerText.Split('-')[1].Trim())
                {
                    case ("Необычная"):
                        Cards[i].Rarity = "uncommon";
                        break;
                    case ("Редкая"):
                        Cards[i].Rarity = "rare";
                        break;
                    case ("Раритетная"):
                        Cards[i].Rarity = "mythic rare";
                        break;
                    default:
                        Cards[i].Rarity = "common";
                        break;
                }
                
                // Формируем стоимость
                foreach (HtmlNode c in htmlDocumentMTG.DocumentNode.SelectNodes("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']" + "//img[@class='Mana']"))
                {
                    Cards[i].Cost = Cards[i].Cost + c.GetAttributeValue("alt", "нету");
                }

                // Формируем цвет
                if (Cards[i].Cost.Trim(new Char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'X', 'Y', 'Z' }).Distinct().Count() <= 1)
                {
                    if (Cards[i].Cost.Contains("W")) Cards[i].Color = "white";
                    else if (Cards[i].Cost.Contains("U")) Cards[i].Color = "blue";
                    else if (Cards[i].Cost.Contains("R")) Cards[i].Color = "red";
                    else if (Cards[i].Cost.Contains("B")) Cards[i].Color = "black";
                    else if (Cards[i].Cost.Contains("G")) Cards[i].Color = "green";
                    else Cards[i].Color = "artifact";
                } else
                {
                    Cards[i].Color = "multicolor";
                }

                //  Формируем изображения
                htmlDocumentWZ.LoadHtml(WZ.DownloadString(WZUrl));
                foreach (HtmlNode x in htmlDocumentWZ.DocumentNode.SelectNodes("//p[@class='rtecenter']"))
                {
                    if (WebUtility.HtmlDecode(x.InnerText).Trim() == Cards[i].NameENG.Trim())
                    {
                        Cards[i].Img = x.SelectNodes("//p[@class='rtecenter']//img[@alt=\"" + Cards[i].NameENG.Trim() + "\"]")[0].GetAttributeValue("src", "не нашел");
                    }
                }
                                
                WebClient myWebClient = new WebClient();
                if (Cards[i].Img != null)
                {
                    var img = new Bitmap(myWebClient.OpenRead(Cards[i].Img));
                    if (!Cards[i].Type.Contains("Сага"))
                    {
                        if (Y == "Y")
                        {
                            img.Clone(new Rectangle(21, 42, 223, 164), img.PixelFormat).Save("c:/sets/" + SetCode + "/" + (i + 1) + ".jpg");
                        }
                        Cards[i].FileImg = (i + 1) + ".jpg";
                    } else {
                        if (Y == "Y")
                        {
                            img.Clone(new Rectangle(132, 42, 111, 268), img.PixelFormat).Save("c:/sets/" + SetCode + "/" + (i + 1) + ".jpg");
                        }
                        Cards[i].FileImg = (i + 1) + ".jpg";
                    }
                }

                // Выбираем шаблон и данные, специфичные для типов
                if (Cards[i].Type.Contains("Сага"))
                    Cards[i].stylesheet = "m15-saga";
                else if (Cards[i].Type.Contains("Planeswalker"))
                    Cards[i].stylesheet = "m15-planeswalker";
                else
                {
                    if (htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoText']//span[@class='rus']") != null)
                    { 
                        Cards[i].RuleText = htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoText']//span[@class='rus']").InnerHtml;
                        Cards[i].RuleText = Cards[i].RuleText.Replace("<p>", Environment.NewLine + "\t\t");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("<i>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("</i>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("<nobr>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("</nobr>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("<u>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("</u>", "");
                        if (htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoText']//span[@class='rus']//img[@class='Mana']") != null)
                        {
                            Cards[i].RuleText = Regex.Replace(Cards[i].RuleText, "<img src=\".* class=\"Mana\">", htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoText']//span[@class='rus']//img[@class='Mana']").GetAttributeValue("alt", ""));
                        }
                    }
                    Cards[i].stylesheet = "m15-flavor-bar";
                }
                // Формируем нумерацию
                if (i < 9)
                    Cards[i].NumberOfSet = "00" + (i + 1) + "/" + MaxSetCard;
                else if (i < 99)
                    Cards[i].NumberOfSet = "0" + (i + 1) + "/" + MaxSetCard;
                else
                    Cards[i].NumberOfSet = (i + 1) + "/" + MaxSetCard;

                // Заполняем
                File.AppendAllText(SetLocation, "card:" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tstylesheet: " + Cards[i].stylesheet + Environment.NewLine);
                File.AppendAllText(SetLocation, "\thas styling: true" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tstyling data:" + Environment.NewLine);
                if (!Cards[i].Type.Contains("Planeswalker") && !Cards[i].Type.Contains("Сага")) { File.AppendAllText(SetLocation, "\t\tchop top: 7" + Environment.NewLine); }
                if (!Cards[i].Type.Contains("Planeswalker") && !Cards[i].Type.Contains("Сага")) { File.AppendAllText(SetLocation, "\t\tchop bottom: 7" + Environment.NewLine); }
                if (!Cards[i].Type.Contains("Planeswalker") && !Cards[i].Type.Contains("Сага")) { File.AppendAllText(SetLocation, "\t\tflavor bar offset: 40" + Environment.NewLine); }
                File.AppendAllText(SetLocation, "\t\ttext box mana symbols: magic-mana-small.mse-symbol-font" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\t\tinverted common symbol: yes" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\t\toverlay:" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tnotes: Создано автоматически" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\ttime created: " + DateTime.Now.ToString("u").Remove(DateTime.Today.ToString("u").Length - 1) + Environment.NewLine);
                File.AppendAllText(SetLocation, "\ttime modified: " + DateTime.Now.ToString("u").Remove(DateTime.Today.ToString("u").Length - 1) + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tcard color: " + Cards[i].Color + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tname: <b>" + Cards[i].Name + "</b>" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tcasting cost: " + Cards[i].Cost + Environment.NewLine);
                File.AppendAllText(SetLocation, "\timage: " + Cards[i].FileImg + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tsuper type: <b>" + Cards[i].Type.Split('-')[0].Trim() + "</b>" + Environment.NewLine);
                if (Cards[i].Type.Split('-').Length > 1) { File.AppendAllText(SetLocation, "\tsub type: <b>" + Cards[i].Type.Split('-')[1].Trim() + "</b>" + Environment.NewLine); }
                File.AppendAllText(SetLocation, "\trarity: " + Cards[i].Rarity + Environment.NewLine);
                File.AppendAllText(SetLocation, "\trule text:\r\n\t\t" + Cards[i].RuleText + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tflavor text:\r\n\t\t" + "Проверочное описание" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tpower: " + Environment.NewLine);
                File.AppendAllText(SetLocation, "\ttoughness: " + Environment.NewLine);

                File.AppendAllText(SetLocation, "\tcustom card number: " + Cards[i].NumberOfSet + Environment.NewLine);
            }
            
            string result = "Сэт обработан";
            return result;
        }

        public static string SetFileJob(string SetCode)
        {

            return "Изменения сохранены";
        }

        static void Main(string[] args)
        {
            string WZUrl = "https://magic.wizards.com/ru/products/dominaria/cards";
            string MTGUrl = "http://www.mtg.ru/cards/search.phtml?Number=";
            string SetCode = "DOM";

            List<Card> m19 = new List<Card>();
            
            DownLoadSet(SetCode, WZUrl, MTGUrl, m19, 280, 269);            

            Console.WriteLine("Тыкни любую пимпу...");
            Console.ReadKey();
        }
    }
}
