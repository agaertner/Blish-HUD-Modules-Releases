using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using static Blish_HUD.GameService;
using static Nekres.Stream_Out.StreamOutModule;
using Graphics = System.Drawing.Graphics;
namespace Nekres.Stream_Out
{
    internal static class Gw2Util
    {
        private static readonly Color Gold = Color.FromArgb(210, 180, 66);
        private static readonly Color Silver = Color.FromArgb(153, 153, 153);
        private static readonly Color Copper = Color.FromArgb(190, 100, 35);
        private static readonly Color Karma = Color.FromArgb(220, 80, 190);

        public static async Task GenerateCoinsImage(string filePath, int coins, bool overwrite = true)
        {
            if (!overwrite && File.Exists(filePath)) return;
            var copper = coins % 100;
            coins = (coins - copper) / 100;
            var silver = coins % 100;
            var gold = (coins - silver) / 100;
            var toDisplay = gold > 0 ? 3 : silver > 0 ? 2 : 1;

            var font = new Font("Arial", 12);

            var copperSize = copper.ToString().Measure(font);
            var silverSize = silver.ToString().Measure(font);
            var goldSize = gold.ToString().Measure(font);

            var fontHeight = Math.Max(Math.Max(silverSize.Height, goldSize.Height), copperSize.Height);

            var copperIconStream = Instance.ContentsManager.GetFileStream("copper_coin.png");
            var copperIcon = new Bitmap(copperIconStream).FitToHeight(fontHeight - 5);
            var silverIconStream = Instance.ContentsManager.GetFileStream("silver_coin.png");
            var silverIcon = new Bitmap(silverIconStream).FitToHeight(fontHeight - 5);
            var goldIconStream = Instance.ContentsManager.GetFileStream("gold_coin.png");
            var goldIcon = new Bitmap(goldIconStream).FitToHeight(fontHeight - 5);

            var margin = 5;
            var width = copperSize.Width + copperIcon.Width;
            if (toDisplay > 1)
                width += margin + silverSize.Width + silverIcon.Width;
            if (toDisplay > 2)
                width += margin + goldSize.Width + goldIcon.Width;

            var height = Math.Max(fontHeight, Math.Max(Math.Max(silverIcon.Height, goldIcon.Height), copperIcon.Height));
            using (var bitmap = new Bitmap(width, height))
            {
                using (var canvas = Graphics.FromImage(bitmap))
                {
                    canvas.SetHighestQuality();

                    var x = 0;
                    var toDraw = toDisplay;
                    while (toDraw > 0)
                    {
                        Bitmap icon;
                        int value;
                        Size size;
                        Color color;
                        switch (toDraw)
                        {
                            case 3:
                                color = Gold;
                                size = goldSize;
                                value = gold;
                                icon = goldIcon;
                                break;
                            case 2:
                                color = Silver;
                                size = silverSize;
                                value = silver;
                                icon = silverIcon;
                                break;
                            default:
                                color = Copper;
                                size = copperSize;
                                value = copper;
                                icon = copperIcon;
                                break;
                        }

                        using (var brush = new SolidBrush(color))
                            canvas.DrawString(value.ToString(), font, brush, x, height / 2 - size.Height / 2);

                        x += toDraw == 3 ? goldSize.Width : toDraw == 2 ? silverSize.Width : copperSize.Width;
                        canvas.DrawImage(icon,
                            new Rectangle(x, height / 2 - icon.Height / 2, icon.Width, icon.Width),
                            new Rectangle(0, 0, icon.Width, icon.Width),
                            GraphicsUnit.Pixel);
                        x += (toDraw == 3 ? goldIcon.Width : toDraw == 2 ? silverIcon.Width : copperIcon.Width) + margin;

                        toDraw--;
                    }
                    canvas.Flush();
                    canvas.Save();
                }
                await bitmap.SaveOnNetworkShare(filePath, ImageFormat.Png);
            }
            copperIcon.Dispose();
            copperIconStream.Close();
            silverIcon.Dispose();
            silverIconStream.Close();
            goldIcon.Dispose();
            goldIconStream.Close();
            font.Dispose();
        }

        public static async Task GenerateKarmaImage(string filePath, int karma, bool overwrite = true)
        {
            if (!overwrite && File.Exists(filePath)) return;
            var font = new Font("Arial", 12);
            var karmaStr = karma.ToString("N0", Overlay.CultureInfo());
            var karmaSize = karmaStr.Measure(font);
            var karmaIconStream = Instance.ContentsManager.GetFileStream("karma.png");
            var karmaIcon = new Bitmap(karmaIconStream).FitToHeight(karmaSize.Height);
            var height = Math.Max(karmaSize.Height, karmaIcon.Height);
            using (var bitmap = new Bitmap(karmaSize.Width + karmaIcon.Width, height))
            {
                using (var canvas = Graphics.FromImage(bitmap))
                {
                    canvas.SetHighestQuality();

                    using (var karmaBrush = new SolidBrush(Karma))
                        canvas.DrawString(karmaStr, font, karmaBrush, 0,
                            karmaSize.Height / 2 - karmaIcon.Height / 2);
                    canvas.DrawImage(karmaIcon,
                        new Rectangle(karmaSize.Width, height / 2 - karmaIcon.Height / 2,
                            karmaIcon.Width, karmaIcon.Width),
                        new Rectangle(0, 0, karmaIcon.Width, karmaIcon.Width),
                        GraphicsUnit.Pixel);
                    canvas.Flush();
                    canvas.Save();
                }
                await bitmap.SaveOnNetworkShare(filePath, ImageFormat.Png);
            }
            karmaIcon.Dispose();
            karmaIconStream.Close();
            font.Dispose();
        }

        public static async Task GeneratePvpTierImage(string filePath, int tier, int maxTiers, bool overwrite = true)
        {
            if (!overwrite && File.Exists(filePath)) return;
            var tierIconFilledStream = Instance.ContentsManager.GetFileStream("1495585.png");
            var tierIconFilled = new Bitmap(tierIconFilledStream);
            var tierIconEmptyStream = Instance.ContentsManager.GetFileStream("1495584.png");
            var tierIconEmpty = new Bitmap(tierIconEmptyStream);

            var width = maxTiers * (Math.Max(tierIconFilled.Width, tierIconEmpty.Width) + 2);
            var height = Math.Max(tierIconFilled.Height, tierIconEmpty.Height);
            using (var bitmap = new Bitmap(width, height))
            {
                using (var canvas = Graphics.FromImage(bitmap))
                {
                    canvas.SetHighestQuality();

                    var drawn = 0;
                    var count = maxTiers;
                    while (drawn < count)
                    {
                        var margin = drawn > 0 ? drawn * 2 : 0;
                        var tierIcon = drawn < tier ? tierIconFilled : tierIconEmpty;
                        canvas.DrawImage(tierIcon,
                            new Rectangle(drawn * tierIcon.Width + margin, height / 2 - tierIcon.Height / 2, tierIcon.Width, tierIcon.Width),
                            new Rectangle(0, 0, tierIcon.Width, tierIcon.Width),
                            GraphicsUnit.Pixel);
                        drawn++;
                    }
                    canvas.Flush();
                    canvas.Save();
                }
                await bitmap.SaveOnNetworkShare(filePath, ImageFormat.Png);
            }
            tierIconFilled.Dispose();
            tierIconFilledStream.Close();
            tierIconEmpty.Dispose();
            tierIconEmptyStream.Close();
        }

        public static DateTime GetDailyResetTime()
        {
            var nextDay = DateTime.UtcNow.AddDays(1);
            return new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 2, 0, 0).ToUniversalTime(); // UTC+2
        }
    }
}
