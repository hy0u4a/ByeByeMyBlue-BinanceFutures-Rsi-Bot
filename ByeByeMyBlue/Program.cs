using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Futures;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ByeByeMyBlue
{
    internal class Program
    {
        #region Setting
        private static string API = "";
        private static string SECRET = "";
        private static int leverage = 5;
        private static decimal fee = 0.98m;
        #endregion

        #region Function
        private static double RSI = 50;
        #endregion

        static void Main(string[] args)
        {
            var Client = new BinanceClient(new BinanceClientOptions
            {
                UsdFuturesApiOptions = new BinanceApiClientOptions
                {
                    ApiCredentials = new ApiCredentials(API, SECRET)
                }
            });

            var balance = Client.UsdFuturesApi.Account.GetAccountInfoAsync();

            new Thread(Start).Start();
            new Thread(CollectData).Start();

            #region ASCII ART
            Console.WriteLine("                             .                                   ");
            Console.WriteLine("                          ,''`.         _                        ");
            Console.WriteLine("                     ,.,'''  '`--- ._,,'|                        ");
            Console.WriteLine("                   ,'                   /                        ");
            Console.WriteLine("              __.-'                    |                         ");
            Console.WriteLine("           ''                ___   ___ |                         ");
            Console.WriteLine("         ,'                 ＼(|＼ /|)/ |                        ");
            Console.WriteLine("        ,'                 _     _     `._                       ");
            Console.WriteLine("       /     ,.......-＼   `.      __     `-.                    ");
            Console.WriteLine("      /     ,' :  .:''`|    `:`.../:.`` ._   `._                 ");
            Console.WriteLine("  ,..,'  _/' .: :'     |     |      '.   ＼.   ＼                ");
            Console.WriteLine(" /      ,'  :'.:  /＼  |     | /＼   ':.  .＼    ＼              ");
            Console.WriteLine("|      /  .: :' ,'  _) \".._,; '  _)    :. :.＼    |             ");
            Console.WriteLine(" |     | :'.:  /   |   .,   /   |       :  :  |    |             ");
            Console.WriteLine(" |     |:' :  /____|  /  ＼ /____|       :  :  |  ,'             ");
            Console.WriteLine("  |   /    '         /    ＼            :'   : |,/               ");
            Console.WriteLine("   ＼ |  '_          /______＼              , : |                ");
            Console.WriteLine("  _/ |  ＼'`--`.    _            ,_   ,-'''  :.|         __      ");
            Console.WriteLine(" /   |   ＼..   ` ./ `.   _,_  ,'  ``'  /'   :'|      _,''/      ");
            Console.WriteLine("/   /'. :   ＼.   _    [_]   `[_]  .__,,|   _....,--=/'  /:       ");
            Console.WriteLine("|   ＼_| :    `.-' `.    _.._     /     . ,'  :. ':/'  /'  `.     ");
            Console.WriteLine("`.   '`'`.         `. ,.'   ` .,'     :'/ ':..':.    |  .:' `.   ");
            Console.WriteLine("  ＼.      ＼          '               :' |    ''''      ''     `. ");
            Console.WriteLine("    `''.   `|        ':     .      .:' ,|         .  ..':.      |");
            Console.WriteLine("      /'   / '' - ..._:   .:'    _;:.,'  ＼.     .:'   :. ''.    |");
            Console.WriteLine("     (._,.'        '`''''''''''''          ＼.._.:      ':  ':   /");
            Console.WriteLine("                                              '`- ._    ,:__,,-' ");
            Console.WriteLine("                                                    ``''         ");
            #endregion

            Console.WriteLine("Balance : " + balance.Result.Data.AvailableBalance + "USDT");
            Console.ReadKey();
        }

        #region Start
        private static void Start()
        {
            var Client = new BinanceClient(new BinanceClientOptions
            {
                UsdFuturesApiOptions = new BinanceApiClientOptions
                {
                    ApiCredentials = new ApiCredentials(API, SECRET)
                }
            });

            decimal positionsize = 0;
            decimal startprice = 0;
            string orderside = null;

            while (true)
            {
                var balance = Client.UsdFuturesApi.Account.GetAccountInfoAsync();
                var ticker = Client.UsdFuturesApi.ExchangeData.GetTickerAsync("BTCUSDT");

                if (balance.Result.Success == true && Convert.ToInt32(balance.Result.Data.AvailableBalance) > 0 && orderside == null)
                {
                    if (RSI < 15)
                    {
                        decimal size = Math.Truncate(Convert.ToDecimal(balance.Result.Data.AvailableBalance) / Convert.ToDecimal(ticker.Result.Data.LastPrice) * fee * leverage * 1000) / 1000;

                        var orderData = Client.UsdFuturesApi.Trading.PlaceOrderAsync(
                            "BTCUSDT",
                            OrderSide.Buy,
                            FuturesOrderType.Market,
                            size);

                        if (orderData.Result.Success == true)
                        {
                            var order = Client.UsdFuturesApi.Trading.GetOrderAsync("BTCUSDT", orderData.Result.Data.Id, orderData.Result.Data.ClientOrderId);

                            startprice += Convert.ToDecimal(order.Result.Data.AvgPrice);
                            positionsize += size;
                            orderside = Convert.ToString(orderData.Result.Data.Side);
                            Log("Position Success : " + orderside + " / Balance : " + balance.Result.Data.AvailableBalance + "USDT" + " / StartPrice : " + startprice);
                        }
                    }
                }

                if (balance.Result.Success == true && Convert.ToInt32(balance.Result.Data.AvailableBalance) > 0 && orderside == "Buy")
                {
                    if (startprice + (startprice * 0.05m / leverage) < ticker.Result.Data.LastPrice || startprice - (startprice * 0.05m / leverage) > ticker.Result.Data.LastPrice)
                    {
                        var orderData = Client.UsdFuturesApi.Trading.PlaceOrderAsync(
                            "BTCUSDT",
                            OrderSide.Sell,
                            FuturesOrderType.Market,
                            positionsize);

                        if (orderData.Result.Success == true)
                        {
                            var order = Client.UsdFuturesApi.Trading.GetOrderAsync("BTCUSDT", orderData.Result.Data.Id, orderData.Result.Data.ClientOrderId);
                            startprice = 0;
                            positionsize = 0;
                            orderside = null;
                            Log("Position Success : " + orderData.Result.Data.Side + " / Balance : " + balance.Result.Data.AvailableBalance + "USDT" + " / ClosePrice : " + order.Result.Data.AvgPrice);
                            Thread.Sleep(60000 * 60);
                        }
                    }
                }
                Thread.Sleep(2000);
            }
        }
        #endregion

        #region Strategy
        private static void CollectData()
        {
            var Client = new BinanceClient(new BinanceClientOptions
            {
                UsdFuturesApiOptions = new BinanceApiClientOptions
                {
                    ApiCredentials = new ApiCredentials(API, SECRET)
                }
            });

            List<double> tickerdata = new List<double>();

            while (true)
            {
                if (DateTime.Now.Minute == 0 || DateTime.Now.Minute == 5 || DateTime.Now.Minute == 10 ||
                    DateTime.Now.Minute == 15 || DateTime.Now.Minute == 20 || DateTime.Now.Minute == 25 ||
                    DateTime.Now.Minute == 30 || DateTime.Now.Minute == 35 || DateTime.Now.Minute == 40 ||
                    DateTime.Now.Minute == 45 || DateTime.Now.Minute == 50 || DateTime.Now.Minute == 55)
                {
                    var ticker = Client.UsdFuturesApi.ExchangeData.GetTickerAsync("BTCUSDT");

                    tickerdata.Add(Convert.ToDouble(ticker.Result.Data.LastPrice));

                    if (tickerdata.Count == 14)
                    {
                        RSI = Rsi(tickerdata);
                        tickerdata.RemoveAt(0);
                    }
                    Thread.Sleep(60000 * 2);
                }
                Thread.Sleep(1000);
            }
        }

        public static double Rsi(List<double> closePrices)
        {
            double sumProfit = 0;
            double sumLoss = 0;
            double Tolerance = 10e-20;

            for (int i = 1; i < closePrices.Count; i++)
            {
                var difference = closePrices[i] - closePrices[i - 1];
                if (difference >= 0)
                {
                    sumProfit += difference;
                }
                else
                {
                    sumLoss -= difference;
                }
            }

            if (sumProfit == 0) return 0;
            if (Math.Abs(sumLoss) < Tolerance) return 100;

            var relativeStrength = (sumProfit / 14) / (sumLoss / 14);

            return 100.0 - (100.0 / (1 + relativeStrength));
        }
        #endregion

        #region Add-On
        private static void Log(string text)
        {
            Console.WriteLine(text);
        }
        #endregion
    }
}
