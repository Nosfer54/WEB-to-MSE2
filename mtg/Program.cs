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
    public string Name, Cost, Img, Type, NumberOfSet, Stats, Rarity, NameENG;
    public Int32 Number;
}

namespace mtg
{
    class Program
    {
        public static string DownLoadSet(string SetCode, string WZUrl, string MTGUrl, List<Card> Cards, int CountCards)
        {
            WebClient MTG = new WebClient();
            WebClient WZ = new WebClient();

            HtmlDocument htmlDocumentMTG = new HtmlDocument();
            HtmlDocument htmlDocumentWZ = new HtmlDocument();

            Console.Clear();
            Console.WriteLine("Для скачивания нажмите Y");
            string Y = Console.ReadLine();

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
                                
                htmlDocumentWZ.LoadHtml(WZ.DownloadString(WZUrl));

                foreach (HtmlNode x in htmlDocumentWZ.DocumentNode.SelectNodes("//p[@class='rtecenter']"))
                {
                    if (WebUtility.HtmlDecode(x.InnerText).Trim() == Cards[i].NameENG.Trim())
                    {
                        Cards[i].Img = x.SelectNodes("//p[@class='rtecenter']//img[@alt=\"" + Cards[i].NameENG.Trim() + "\"]")[0].GetAttributeValue("src", "не нашел");
                    }
                }

                Directory.CreateDirectory("c:/sets/" + SetCode + "/");
                WebClient myWebClient = new WebClient();

                if ((Cards[i].Img != null) && (Y != "y"))
                {
                    var img = new Bitmap(myWebClient.OpenRead(Cards[i].Img));
                    if (!Cards[i].Type.Contains("Сага"))
                    {
                        img.Clone(new Rectangle(21, 42, 223, 164), img.PixelFormat).Save("c:/sets/" + SetCode + "/" + (i + 1) + ".jpg");
                    } else {
                        img.Clone(new Rectangle(132, 42, 111, 268), img.PixelFormat).Save("c:/sets/" + SetCode + "/"+ (i + 1) + ".jpg");
                    }
                }
            }

            string result = "Сэт обработан";
            return result;
        }

        static void Main(string[] args)
        {
            string WZUrl = "https://magic.wizards.com/ru/products/dominaria/cards";
            string MTGUrl = "http://www.mtg.ru/cards/search.phtml?Number=";

            List<Card> m19 = new List<Card>();

            DownLoadSet("DOM", WZUrl, MTGUrl, m19, 280);
            
            Console.WriteLine("Тыкни любую пимпу...");
            Console.ReadKey();
        }
    }
}
