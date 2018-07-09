using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using System.IO;
using System.IO.Compression;
using System.Drawing;


public class Card
{
    public string Name, Cost, Img, Type, NumberOfSet, Stats, Rarity, NameENG, stylesheet, FileImg;
    public Int32 Number;
}

namespace mtg
{
    class Program
    {
        public static string DownLoadSet(string SetCode, string WZUrl, string MTGUrl, List<Card> Cards, int CountCards, int MaxSetCard)
        {
            string SetLocation = "c:/sets/" + SetCode + "/" + SetCode;
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
                Cards[i].Rarity = htmlDocumentMTG.DocumentNode.SelectNodes("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']")[4].InnerText;

                foreach (HtmlNode c in htmlDocumentMTG.DocumentNode.SelectNodes("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']" + "//img[@class='Mana']"))
                {
                    Cards[i].Cost = Cards[i].Cost + c.GetAttributeValue("alt", "нету");
                }

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

                if (Cards[i].Type.Contains("Сага"))
                    Cards[i].stylesheet = "magic-m15-saga";
                else if (Cards[i].Type.Contains("Plan"))
                    Cards[i].stylesheet = "m15-planeswalker";
                else
                    Cards[i].stylesheet = "m15-flavor-bar";

                if (i < 10)
                    Cards[i].NumberOfSet = "00" + i + "/" + MaxSetCard;
                else if (i < 100)
                    Cards[i].NumberOfSet = "0" + i + "/" + MaxSetCard;
                else
                    Cards[i].NumberOfSet = i + "/" + MaxSetCard;

                File.AppendAllText(SetLocation, "card:" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tstylesheet: " + Cards[i].stylesheet + Environment.NewLine);
                File.AppendAllText(SetLocation, "\thas styling: true" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tstyling data:" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\t\tchop top: 7" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\t\tchop bottom: 7" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\t\tflavor bar offset: 40" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\t\ttext box mana symbols: magic-mana-small.mse-symbol-font" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\t\tinverted common symbol: yes" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\t\toverlay:" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tnotes: Создано автоматически" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\ttime created: " + DateTime.Now.ToString("u").Remove(DateTime.Today.ToString("u").Length - 1) + Environment.NewLine);
                File.AppendAllText(SetLocation, "\ttime modified: " + DateTime.Now.ToString("u").Remove(DateTime.Today.ToString("u").Length - 1) + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tname: <b>" + Cards[i].Name + "</b>" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tcasting cost: " + Cards[i].Cost + Environment.NewLine);
                File.AppendAllText(SetLocation, "\timage: " + Cards[i].FileImg + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tsuper type: " + Cards[i].Type.Split('-')[0].Trim() + Environment.NewLine);
                if (Cards[i].Type.Split('-').Length > 1)
                {
                    File.AppendAllText(SetLocation, "\tsub type: " + Cards[i].Type.Split('-')[1].Trim() + Environment.NewLine);
                }
                File.AppendAllText(SetLocation, "\trule text: " + "Проверочный текст (всякий разный)" + Environment.NewLine);
                File.AppendAllText(SetLocation, "\tflavor text: " + "Проверочное описание" + Environment.NewLine);
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
