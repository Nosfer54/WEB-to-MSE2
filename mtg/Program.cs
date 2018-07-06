using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using System.IO;
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
            WebClient WZ = new WebClient();
            WebClient MTG = new WebClient();

            HtmlDocument htmlDocumentWZ = new HtmlDocument();
            htmlDocumentWZ.LoadHtml(WZUrl);

            for (int i = 0; i < CountCards; i++)
            {
                Cards.Add(new Card);
                Console.WriteLine("Это строка номер: " + (i + 1));
            }

            string result = "Изображения сета " + SetCode + "скачаны";
            return result;
        }

        static void Main(string[] args)
        {
            string WZUrl = "https://magic.wizards.com/ru/products/dominaria/cards";
            string MTGUrl = "http://www.mtg.ru/cards/search.phtml?Number=";

            List<Card> m19 = new List<Card>();

            DownLoadSet("DOM", WZUrl, MTGUrl, m19, 280);


            /*WebClient wc = new WebClient();
            WebClient wz = new WebClient();

            HtmlDocument htmlDocumentWZ = new HtmlDocument();
            htmlDocumentWZ.LoadHtml(wz.DownloadString(""));

            for (int i = 0; i < 280; i++)
            {
                m19.Add(new Card());

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(wc.DownloadString("http://www.mtg.ru/cards/search.phtml?Number=" + (i + 1) + "&Grp=DOM"));

                var N = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']" + "//h2").InnerText;

                m19[i].Name = (N.Substring(N.IndexOf("//") + 3, N.Length - N.IndexOf("//") - 3)).Trim();
                m19[i].NameENG = (N.Substring(0, N.IndexOf("//"))).Trim();

                m19[i].Type = (htmlDocument.DocumentNode.SelectSingleNode("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']" + "//span[@class='rus']").InnerText).Substring(2);
                m19[i].NumberOfSet = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='NoteDiv U shadow']" + "//span[@class='bold2']").InnerText;
                m19[i].Rarity = htmlDocument.DocumentNode.SelectNodes("//table[@class='NoteDiv U shadow']" + "//div[@class='SearchCardInfoDIV shadow']")[4].InnerText;

                foreach (HtmlNode x in htmlDocumentWZ.DocumentNode.SelectNodes("//p[@class='rtecenter']"))
                {
                    if (WebUtility.HtmlDecode(x.InnerText).Trim() == m19[i].NameENG.Trim())
                    {
                        m19[i].Img = x.SelectNodes("//p[@class='rtecenter']//img[@alt=\"" + m19[i].NameENG.Trim() + "\"]")[0].GetAttributeValue("src","не нашел");
                    } 
                }

                string numb;
                
                if (i < 10)
                {
                    numb = "00" + (i + 1);
                } else if (i < 100)
                {
                    numb = "0" + (i + 1);
                } else
                {
                    numb = i.ToString();
                }

                Directory.CreateDirectory("c:/DOM/");
                WebClient myWebClient = new WebClient();

                if (m19[i].Img != null)
                {
                    var img = new Bitmap(myWebClient.OpenRead(m19[i].Img));
                    if (!m19[i].Type.Contains("Сага"))
                    {
                        img.Clone(new Rectangle(21, 42, 223, 164), img.PixelFormat).Save("c:/DOM/" + (i + 1) + ".jpg");                        
                    } else
                    {
                        img.Clone(new Rectangle(132, 42, 111, 268), img.PixelFormat).Save("c:/DOM/" + (i + 1) + ".jpg");
                    }
                }

                //Console.Clear();
                Console.WriteLine(m19[i].Type);
                Console.WriteLine("Получаем " + (i + 1) + " из 280. Скачиваем...");


            }*/

            Console.WriteLine("Тыкни любую пимпу...");
            Console.ReadKey();
        }
    }
}
