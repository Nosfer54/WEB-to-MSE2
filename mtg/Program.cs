using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Text.RegularExpressions;

public class Card
{
    public string Name, Cost, Img, Type, NumberOfSet, Stats, Rarity, NameENG, stylesheet, FileImg, Color, 
                    RuleText, FlavorText, loyalty, Illustrator, Power, Toughness, TextBoxes, NumberCoordinates, DividerCoordinates, SpecialText;
    public Int32 Number;
    public List<string> LoyaltyCost = new List<string>();
    public List<string> LevelText = new List<string>();
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
                Cards[i].Illustrator = htmlDocumentMTG.DocumentNode.SelectSingleNode("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIVsub shadow']//table[@class='NoteDivWidth']//tr//td").InnerText.Split(':')[1].Trim();

                if (!htmlDocumentMTG.DocumentNode.SelectNodes("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']")[4].InnerText.Contains("Базовая земля"))
                { 
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
                } else
                {
                    Cards[i].Rarity = "common";
                }

                // Формируем стоимость
                if (htmlDocumentMTG.DocumentNode.SelectNodes("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']" + "//img[@class='Mana']") != null)
                {
                    foreach (HtmlNode c in htmlDocumentMTG.DocumentNode.SelectNodes("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']" + "//img[@class='Mana']"))
                    {
                        Cards[i].Cost = Cards[i].Cost + c.GetAttributeValue("alt", "нету");
                    }
                }

                // Формируем цвет
                if (Cards[i].Cost == null)
                {
                    Cards[i].Color = "land";
                }
                else if (Cards[i].Cost.Trim(new Char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'X', 'Y', 'Z' }).Distinct().Count() <= 1)
                {
                    if (Cards[i].Cost.Contains("W")) Cards[i].Color = "white";
                    else if (Cards[i].Cost.Contains("U")) Cards[i].Color = "blue";
                    else if (Cards[i].Cost.Contains("R")) Cards[i].Color = "red";
                    else if (Cards[i].Cost.Contains("B")) Cards[i].Color = "black";
                    else if (Cards[i].Cost.Contains("G")) Cards[i].Color = "green";
                    else Cards[i].Color = "artifact";
                }
                else
                {
                    Cards[i].Color = "multicolor";
                }

                //  Формируем изображения
                htmlDocumentWZ.LoadHtml(WZ.DownloadString(WZUrl));
                foreach (HtmlNode x in htmlDocumentWZ.DocumentNode.SelectNodes("//p[@class='rtecenter']"))
                {
                    if (WebUtility.HtmlDecode(x.InnerText).Trim().Replace('’', '\'') == Cards[i].NameENG.Trim())
                    {
                        try
                        {
                            Cards[i].Img = x.SelectNodes("//p[@class='rtecenter']//img[@alt=\"" + Cards[i].NameENG.Trim().Replace("'", "&rsquo;") + "\"]")[0].GetAttributeValue("src", "не нашел");
                        }
                        catch (System.NullReferenceException)
                        {
                            Console.WriteLine("Английское имя: " + Cards[i].NameENG.Trim());
                            Console.WriteLine("Не смог сопоставить с: " + x.InnerText);
                            Console.WriteLine("Тыкни любую пимпу...");
                            Console.ReadKey();
                        }
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
                { 
                    Cards[i].stylesheet = "m15-saga";
                    Cards[i].RuleText = "<i-auto>(При выходе этой Саги и после вашего шага взятия карты добавьте один жетон знаний. Пожертвуйте после III.)</i-auto>";
                    Cards[i].FlavorText = null;

                    string str = htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoText']//span[@class='rus']").InnerHtml.Trim();
                    
                    switch (Regex.Split(str, "<p>").Count() - 1)
                    {
                        case 2:
                            Cards[i].TextBoxes = "two";
                            Cards[i].NumberCoordinates = "183,223,329,";
                            Cards[i].DividerCoordinates = "296,";
                            break;
                        case 3:
                            Cards[i].TextBoxes = "three";
                            Cards[i].NumberCoordinates = "185,279,373,";
                            Cards[i].DividerCoordinates = "249,343,";
                            break;
                        case 4:
                            Cards[i].TextBoxes = "four";
                            Cards[i].NumberCoordinates = "185,279,373,450,";
                            Cards[i].DividerCoordinates = "225,296,367,";
                            break;
                    }

                    Cards[i].SpecialText = Environment.NewLine + "\t\t" + Cards[i].RuleText;

                    string LevelText = htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoText']//span[@class='rus']").InnerHtml.Trim();
                    for (int k = 1; k < Regex.Split(str, "<p>").Count(); k++)
                    {
                        Cards[i].LevelText.Add(Regex.Split(Regex.Split(LevelText, "<p>")[k], " — ")[1]);
                        Cards[i].SpecialText = Cards[i].SpecialText + Environment.NewLine + "\t\t" + Regex.Split(LevelText, "<p>")[k];
                    }
                }   
                else if (Cards[i].Type.Contains("Planeswalker"))
                {
                    Cards[i].stylesheet = "m15-planeswalker";
                    Cards[i].loyalty = htmlDocumentMTG.DocumentNode.SelectNodes("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']")[3].InnerText.Split(':')[1].Trim();

                    if (htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoText']//span[@class='rus']") != null)
                    {
                        Cards[i].RuleText = "";
                        string str = htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoText']//span[@class='rus']").InnerHtml.Trim();

                        for (int k = 0; k < Regex.Split(str, "<p>").Count(); k++)
                        {
                            if (k == 0)
                            {
                                Cards[i].RuleText = Cards[i].RuleText + Regex.Split(Regex.Split(str, "<p>")[k], ": ")[1];
                            } else
                            {
                                Cards[i].RuleText = Cards[i].RuleText + Environment.NewLine + "\t\t" + Regex.Split(Regex.Split(str, "<p>")[k], ": ")[1];
                            }
                            Cards[i].LoyaltyCost.Add(Regex.Split(Regex.Split(str, "<p>")[k], ": ")[0]);
                        }
                        Cards[i].FlavorText = null;
                    }
                }
                else
                {
                    if (htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoText']//span[@class='rus']") != null)
                    {
                        Cards[i].RuleText = htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoText']//span[@class='rus']").InnerHtml;
                        foreach (Match rt in Regex.Matches(Cards[i].RuleText, "<img src=\".*? class=\"Mana\">"))
                        {
                            HtmlDocument a = new HtmlDocument();
                            a.LoadHtml(rt + "");
                            if (a.DocumentNode.SelectSingleNode("//img").GetAttributeValue("alt", "") == "TAP")
                            {
                                Cards[i].RuleText = Cards[i].RuleText.Replace(rt + "", "<sym>T</sym>");
                            } else
                            {
                                Cards[i].RuleText = Cards[i].RuleText.Replace(rt + "", "<sym>" + a.DocumentNode.SelectSingleNode("//img").GetAttributeValue("alt", "") + "</sym>");
                            }
                        }
                        Cards[i].RuleText = Cards[i].RuleText.Replace("<p>", Environment.NewLine + "\t\t");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("<i>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("<i>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("</i>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("<nobr>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("</nobr>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("<u>", "");
                        Cards[i].RuleText = Cards[i].RuleText.Replace("</u>", "");
                    }
                    if (htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoDIVsub SearchCardTextDiv shadow']//span[@class='rus']") != null)
                    {
                        Cards[i].FlavorText = htmlDocumentMTG.DocumentNode.SelectSingleNode("//div[@class='SearchCardInfoDIVsub SearchCardTextDiv shadow']//span[@class='rus']").InnerHtml;
                        Cards[i].FlavorText = Cards[i].FlavorText.Replace("<p>", Environment.NewLine + "\t\t");
                        Cards[i].FlavorText = Cards[i].FlavorText.Replace("<i>", "");
                        Cards[i].FlavorText = Cards[i].FlavorText.Replace("</i>", "");
                        Cards[i].FlavorText = Cards[i].FlavorText.Replace("<nobr>", "");
                        Cards[i].FlavorText = Cards[i].FlavorText.Replace("</nobr>", "");
                        Cards[i].FlavorText = Cards[i].FlavorText.Replace("<u>", "");
                        Cards[i].FlavorText = Cards[i].FlavorText.Replace("</u>", "");
                    }

                    if (Cards[i].Type.Contains("Легендарн"))
                    {
                        Cards[i].stylesheet = "m15-legendary";
                    }
                    else
                    {
                        Cards[i].stylesheet = "m15-flavor-bar";
                    }

                    if (Cards[i].Type.Contains("Существо"))
                    {
                        string PowerToughness = htmlDocumentMTG.DocumentNode.SelectNodes("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']")[3].InnerText.Split(':')[1].Trim();
                        Cards[i].Power = PowerToughness.Split('/')[0].Trim();
                        Cards[i].Toughness = PowerToughness.Split('/')[1].Trim();
                    }
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
                if (!Cards[i].Type.Contains("Planeswalker") && !Cards[i].Type.Contains("Сага")) { File.AppendAllText(SetLocation, "\t\tflavor bar offset: 0" + Environment.NewLine); }
                if (Cards[i].Type.Contains("Сага")) { File.AppendAllText(SetLocation, "\t\tchapter textboxes: " + Cards[i].TextBoxes + Environment.NewLine); }
                if (Cards[i].Type.Contains("Сага")) { File.AppendAllText(SetLocation, "\t\tchapter number coordinates: " + "208,329,370," + Environment.NewLine); }
                File.AppendAllText(SetLocation, "\t\ttext box mana symbols: magic-mana-small.mse-symbol-font" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\t\tinverted common symbol: yes" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\t\toverlay:" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tnotes: Создано автоматически" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\ttime created: " + DateTime.Now.ToString("u").Remove(DateTime.Today.ToString("u").Length - 1) + Environment.NewLine);
                File.AppendAllText(SetLocation, "\ttime modified: " + DateTime.Now.ToString("u").Remove(DateTime.Today.ToString("u").Length - 1) + Environment.NewLine);
                if (Cards[i].Type.Contains("Сага")) { File.AppendAllText(SetLocation, "\textra data:" + Environment.NewLine + 
                    "\t\tmagic-m15-saga: chapter text: " + Cards[i].RuleText + Environment.NewLine); }
                File.AppendAllText(SetLocation, "\tcard color: " + Cards[i].Color + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tname: <b>" + Cards[i].Name + "</b>" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tcasting cost: " + Cards[i].Cost + Environment.NewLine);
                File.AppendAllText(SetLocation, "\timage: " + Cards[i].FileImg + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tsuper type: <b>" + Cards[i].Type.Split('-')[0].Trim() + "</b>" + Environment.NewLine);
                if (Cards[i].Type.Split('-').Length > 1) { File.AppendAllText(SetLocation, "\tsub type: <b>" + Cards[i].Type.Split('-')[1].Trim() + "</b>" + Environment.NewLine); }
                File.AppendAllText(SetLocation, "\trarity: " + Cards[i].Rarity + Environment.NewLine);
                File.AppendAllText(SetLocation, "\trule text:\r\n\t\t" + Cards[i].RuleText + Environment.NewLine);
                if (!Cards[i].Type.Contains("Planeswalker") && !Cards[i].Type.Contains("Сага")) { File.AppendAllText(SetLocation, "\tflavor text:\r\n\t\t" + Cards[i].FlavorText + Environment.NewLine); }
                if (Cards[i].Type.Contains("Существо")) { File.AppendAllText(SetLocation, "\tpower: " + Cards[i].Power + Environment.NewLine); }
                if (Cards[i].Type.Contains("Существо")) { File.AppendAllText(SetLocation, "\ttoughness: " + Cards[i].Toughness + Environment.NewLine); }
                if (Cards[i].Type.Contains("Planeswalker")) { File.AppendAllText(SetLocation, "\tloyalty: " + Cards[i].loyalty + Environment.NewLine); }
                for (int lc = 0; lc < Cards[i].LoyaltyCost.Count(); lc++)
                {
                    File.AppendAllText(SetLocation, "\tloyalty cost " + (lc + 1) + ": " + Cards[i].LoyaltyCost[lc] + Environment.NewLine);
                }
                File.AppendAllText(SetLocation, "\tcustom card number: " + Cards[i].NumberOfSet + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tillustrator: " + Cards[i].Illustrator + Environment.NewLine);
                if (Cards[i].Type.Contains("Сага")) { File.AppendAllText(SetLocation, "\tspecial text: " + Cards[i].SpecialText + Environment.NewLine); }
                for (int lc = 0; lc < Cards[i].LevelText.Count(); lc++)
                {
                    File.AppendAllText(SetLocation, "\tlevel " + (lc + 1) + " text: " + Cards[i].LevelText[lc] + Environment.NewLine);
                }
                //File.AppendAllText(SetLocation, "\tcopyright: ™ & © 2018 Wizards of the Coast" + Environment.NewLine);
                //File.AppendAllText(SetLocation, "\tcopyright 2: ™ & © 2018 Wizards of the Coast" + Environment.NewLine);
                //File.AppendAllText(SetLocation, "\tcopyright 3: ™ & © 2018 Wizards of the Coast" + Environment.NewLine);                
            }

            ZipFile.CreateFromDirectory("c:/sets/" + SetCode + "/", "c:/sets/" + SetCode + ".mse-set", CompressionLevel.NoCompression,false);

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

            List<Card> SetCards = new List<Card>();
            
            DownLoadSet(SetCode, WZUrl, MTGUrl, SetCards, 280, 269);            

            Console.WriteLine("Тыкни любую пимпу...");
            Console.ReadKey();
        }
    }
}
