using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxNSTOrdersAPI;
using NSTOrdersAPI;
using MtApi;
using MtApi.Monitors;

namespace MTAPITradePump
{
    
    public partial class Form1 : Form
    {

        private AxNSTOrdersAPI.AxNSTOrders _AxNSTOrders;
        private MtApiClient _apiClient;
        private TimerTradeMonitor _timerTradeMonitor;
        private TimeframeTradeMonitor _timeframeTradeMonitor;

        private double minLot;
        private double maxLot;
        private int minLotPt;
        private string acctCurr;

        private const short MARKET_ORDER = 1;
        private const short STOP_ORDER = 2;
        private const short LIMIT_ORDER = 3;
        private const short STOPLIMIT_ORDER = 4;
        private const short MARKETCLOSE_ORDER = 5;

        private const short LONGENTRY_ACTION = 1;
        private const short SHORTENTRY_ACTION = 2;
        private const short LONGEXIT_ACTION = 3;
        private const short SHORTEXIT_ACTION = 4;

        public void initMTAPI()
        {
            this._apiClient = new MtApiClient();
            this._apiClient.BeginConnect(Properties.Settings.Default.MTAPIhost,
                                         Properties.Settings.Default.MTAPIport);
            _timerTradeMonitor = new TimerTradeMonitor(_apiClient) { Interval = 10000 }; // 10 sec
            _timerTradeMonitor.AvailabilityOrdersChanged += _tradeMonitor_AvailabilityOrdersChanged;

            _timeframeTradeMonitor = new TimeframeTradeMonitor(_apiClient);
            _timeframeTradeMonitor.AvailabilityOrdersChanged += _tradeMonitor_AvailabilityOrdersChanged;
        }

        private MtApi.TradeOperation xlateEventActions(int Action, int OrderType)
        {
            if (Action == LONGENTRY_ACTION && OrderType == MARKET_ORDER)
                return TradeOperation.OP_BUY;
            else if(Action == SHORTENTRY_ACTION && OrderType == MARKET_ORDER)
                return TradeOperation.OP_SELL;
            else if (Action == LONGENTRY_ACTION && OrderType == STOP_ORDER)
                return TradeOperation.OP_BUYSTOP;
            else if (Action == SHORTENTRY_ACTION && OrderType == STOP_ORDER)
                return TradeOperation.OP_SELLSTOP;
            else if (Action == LONGENTRY_ACTION && OrderType == LIMIT_ORDER)
                return TradeOperation.OP_BUYLIMIT;
            else if (Action == SHORTENTRY_ACTION && OrderType == LIMIT_ORDER)
                return TradeOperation.OP_SELLLIMIT;
            return (MtApi.TradeOperation)(-1);
        }

        public Form1()
        {
            initMTAPI();
            //
            InitializeComponent();
        }

        private void _AxNSTOrders_Logon(object sender, __NSTOrders_LogonEvent e)
        {
            bool result = _apiClient.IsConnected();
            if (result == false)
            {
                e.loggedOn = 0;
                LogMessage("result: false");
            }
            //e.loggedOn = 0;
            LogMessage($"logon: {e.loggedOn}");
        }

        private void _AxNSTOrders_NewOrder(object sender, __NSTOrders_NewOrderEvent e)
        {
            int slose=0;
            int tic=-1;
            double tmp = 0.0;

            //must pass shares as account dollar/base current amount from NST in the trading system
            double slprice = e.stopPrice;
            if (e.action == LONGENTRY_ACTION)
                tmp = Math.Round(Math.Abs(_apiClient.MarketInfo(e.ticker, MarketInfoModeType.MODE_BID) - slprice),0);
            else if (e.action == SHORTENTRY_ACTION)
                tmp = Math.Round(Math.Abs(_apiClient.MarketInfo(e.ticker, MarketInfoModeType.MODE_ASK) - slprice),0);
            slose = Convert.ToInt32(tmp);
            double vol = calcSLlotsSi(e.ticker, slose, e.shares);

            //double vol = e.shares * Properties.Settings.Default.MTAPI_NS_Lot_multplier;
            if (vol < minLot)
                vol = minLot;
            else if (vol > maxLot)
                vol = maxLot;
            vol = Math.Round(vol, minLotPt);
            _apiClient.Print("_AxNSTOrders_NewOrder");
            LogMessage($"try opening ticket={e.orderId} ticker={e.ticker} e.shares={e.shares} vol={vol} limitprice={e.limitPrice} stop loss={e.stopPrice}");
            _apiClient.Print($"try opening ticket={e.orderId} ticker={e.ticker} e.shares={e.shares} vol={vol} limitprice={e.limitPrice} stop loss={e.stopPrice}");

            if (e.action == LONGENTRY_ACTION)
                tic = _apiClient.OrderSendBuy(e.ticker, vol, Properties.Settings.Default.MTAPIslip, e.stopPrice, 0.0);
            else if (e.action == SHORTENTRY_ACTION)
                tic = _apiClient.OrderSendSell(e.ticker, vol, Properties.Settings.Default.MTAPIslip, e.stopPrice, 0.0);
            if (tic != -1)
            {
                LogMessage($"new order completed ticket={e.orderId} new tic={tic} ticker={e.ticker} vol={vol} limitprice={e.limitPrice} stop loss={e.stopPrice}");
                _apiClient.Print($"new order completed ticket={e.orderId} new tic={tic} ticker={e.ticker} vol={vol} limitprice={e.limitPrice} stop loss={e.stopPrice}");
                _apiClient.OrderSelect(tic, MtApi.OrderSelectMode.SELECT_BY_TICKET);
                _apiClient.OrderPrint();
            }
            else
            {
                tic = 0;
                LogMessage("No ticket");
                _apiClient.Print("No ticket");
            }
            e.orderId = Convert.ToString(tic);
        }

        private void _AxNSTOrders_CancelOrder(object sender, __NSTOrders_CancelOrderEvent e)
        {
            _apiClient.OrderClose(Convert.ToInt32(e.orderId), Properties.Settings.Default.MTAPIslip);
        }
        
        private void _AxNSTOrders_ModifyOrder(object sender, __NSTOrders_ModifyOrderEvent e)
        {
            DateTime dt = DateTime.Now.AddMinutes(1);
            int tic = Convert.ToInt32(e.orderId);
            
            _apiClient.OrderModify(tic,0.0,e.stopPrice,e.limitPrice, dt);
        }

        private void _AxNSTOrders_OrderStatus(object sender, __NSTOrders_OrderStatusEvent e)
        {
            string stype;
            MtApi.MtOrder order = _apiClient.GetOrder(Convert.ToInt32(e.orderId), MtApi.OrderSelectMode.SELECT_BY_TICKET, MtApi.OrderSelectSource.MODE_TRADES);
            if (order.Operation == MtApi.TradeOperation.OP_BUY)
                stype = "BUY";
            else
                stype = "SELL";
            LogMessage($"ticket={e.orderId} type={stype} price={order.OpenPrice} stop loss={order.StopLoss} take profit={order.TakeProfit}");
            _apiClient.Print($"ticket={e.orderId} type={stype} price={order.OpenPrice} stop loss={order.StopLoss} take profit={order.TakeProfit}");
        }

        private void _AxNSTOrders_VerifyTicker(object sender, __NSTOrders_VerifyTickerEvent e)
        {
            //e.ticker
            if (_apiClient.MarketInfo(e.ticker, MtApi.MarketInfoModeType.MODE_HIGH) > 0.0)
            {
                e.valid = -1;
                LogMessage($"valid ticker: {e.ticker}");
            }
            else
            {
                LogMessage($"invalid ticker: {e.ticker}");
                e.valid = 0;
            }
        }

        private void _tradeMonitor_AvailabilityOrdersChanged(object sender, AvailabilityOrdersEventArgs e)
        {
            if (e.Opened != null)
            {
                LogMessage($"{sender.GetType()}: Opened orders - {string.Join(", ", e.Opened.Select(o => o.Ticket).ToList())}");
            }

            if (e.Closed != null)
            {
                LogMessage($"{sender.GetType()}: Closed orders - {string.Join(", ", e.Closed.Select(o => o.Ticket).ToList())}");
            }
        }

        public void LogMessage(string message)
        {
            logListBox.Items.Add(message);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            minLot = _apiClient.MarketInfo("", MarketInfoModeType.MODE_MINLOT);
            maxLot = _apiClient.MarketInfo("", MarketInfoModeType.MODE_MAXLOT);
            minLotPt = (-1) * (int)Math.Log10(minLot);
            acctCurr = _apiClient.AccountCurrency();
            LogMessage($"Account Company: {_apiClient.AccountCompany()}");
            LogMessage($"Account Server: {_apiClient.AccountServer()}");
            LogMessage($"Account Name: {_apiClient.AccountName()}");
            LogMessage($"Account Number: {_apiClient.AccountNumber()}");
            LogMessage($"Account Currency: {acctCurr}");
            LogMessage($"Account Balance: {_apiClient.AccountBalance()}");
            LogMessage($"Account Equity: {_apiClient.AccountEquity()}");
            LogMessage($"Minimum lot size: {minLot}");
            LogMessage($"Maximum lot size: {maxLot}");
            LogMessage($"Minimum lot dec pt: {minLotPt}");
            LogMessage($"Test calcSLlotsSi for EURGBP at $1.75: {calcSLlotsSi("EURGBP",0,1.75)}");
        }

        private double calcSLlotsSi(string sym,
                                    int slose,
                                    double amtRisked)
        {
            double posSize,
                   pipValue,
                   trueSL,
                   cross1,
                   cross2,
                   tmp;
            double tickSize = _apiClient.MarketInfo(sym, MarketInfoModeType.MODE_TICKSIZE),
                   lotSize = _apiClient.MarketInfo(sym, MarketInfoModeType.MODE_LOTSIZE),
                   spr = _apiClient.MarketInfo(sym, MarketInfoModeType.MODE_SPREAD),
                   priceVal = _apiClient.MarketInfo(sym, MarketInfoModeType.MODE_BID);
            string baseCurr = sym.Substring(0, 3),
                   quoteCurr = sym.Substring(3, 3);

            trueSL = slose + spr;
            if (slose != 0)
                pipValue = amtRisked / slose;
            else
                pipValue = amtRisked;
            posSize = amtRisked / tickSize / trueSL;

           if (baseCurr == acctCurr)  // Indirect Rate , e.g. deposit currency is USD and using USD/JPY or USD/CHF
                posSize *= priceVal;
            else  // cross rate
            {
                cross1 = _apiClient.MarketInfo(quoteCurr + acctCurr, MarketInfoModeType.MODE_BID);
                cross2 = _apiClient.MarketInfo(acctCurr + quoteCurr, MarketInfoModeType.MODE_BID);
                if (cross1 > 0) // cross rate where Acct currency is a QUOTE for the current rate's QUOTE (e.g., EUR/GBP  ==>  GBP/USD)
                    posSize /= cross1;
                else if (cross2 > 0)  // cross rate where Acct currency is a BASE for the current rate's QUOTE (e.g., EUR/JPY  ==>  USD/JPY)
                    posSize *= cross2;
            }

            tmp = Math.Round(posSize / lotSize,
                             minLotPt);
            return tmp;
        }
    }
}